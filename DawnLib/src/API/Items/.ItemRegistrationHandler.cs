using System.Collections.Generic;
using System.Linq;
using Dawn.Internal;
using Dawn.Utils;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace Dawn;

static class ItemRegistrationHandler
{
    internal static void Init()
    {
        LethalContent.Items.AddAutoTaggers(
            new AutoNonInteractableTagger(),
            new SimpleAutoTagger<DawnItemInfo>(Tags.Conductive, itemInfo => itemInfo.Item.isConductiveMetal),
            new SimpleAutoTagger<DawnItemInfo>(Tags.Noisy, itemInfo => itemInfo.Item.spawnPrefab.GetComponent<NoisemakerProp>() != null),
            new SimpleAutoTagger<DawnItemInfo>(Tags.Interactable, itemInfo => !itemInfo.HasTag(Tags.NonInteractable) && !itemInfo.HasTag(Tags.Noisy)),
            new SimpleAutoTagger<DawnItemInfo>(Tags.Paid, itemInfo => itemInfo.ShopInfo != null),
            new SimpleAutoTagger<DawnItemInfo>(Tags.Scrap, itemInfo => itemInfo.ScrapInfo != null),
            new SimpleAutoTagger<DawnItemInfo>(Tags.Chargeable, itemInfo => itemInfo.Item.requiresBattery),
            new SimpleAutoTagger<DawnItemInfo>(Tags.TwoHanded, itemInfo => itemInfo.Item.twoHanded),
            new SimpleAutoTagger<DawnItemInfo>(Tags.OneHanded, itemInfo => !itemInfo.Item.twoHanded),
            new SimpleAutoTagger<DawnItemInfo>(Tags.Weapon, itemInfo => itemInfo.Item.isDefensiveWeapon),
            new AutoItemGroupTagger(Tags.GeneralItemClassGroup, "GeneralItemClass"),
            new AutoItemGroupTagger(Tags.SmallItemsGroup, "SmallItems"),
            new AutoItemGroupTagger(Tags.TabletopItemsGroup, "TabletopItems"),
            new AutoItemGroupTagger(Tags.TestItemGroup, "TestItem"),
            new AutoValueTagger(Tags.LowValue, new BoundedRange(0, 100)),
            new AutoValueTagger(Tags.MediumValue, new BoundedRange(100, 200)),
            new AutoValueTagger(Tags.HighValue, new BoundedRange(200, int.MaxValue)),
            new AutoWeightTagger(Tags.LightWeight, new BoundedRange(0, 1.2f)),
            new AutoWeightTagger(Tags.MediumWeight, new BoundedRange(1.2f, 1.4f)),
            new AutoWeightTagger(Tags.HeavyWeight, new BoundedRange(1.4f, int.MaxValue))
        );

        using (new DetourContext(priority: int.MaxValue - 9))
        {
            On.StartOfRound.Awake += RegisterScrapItems;
            On.Terminal.Awake += RegisterShopItemsToTerminal;
        }

        using (new DetourContext(priority: int.MinValue))
        {
            On.RoundManager.SpawnScrapInLevel += UpdateItemWeights;
        }

        On.StartOfRound.SetPlanetsWeather += UpdateItemWeights;
        LethalContent.Moons.OnFreeze += FreezeItemContent;
        LethalContent.Items.OnFreeze += RedoItemsDebugMenu;
    }

    private static void RegisterScrapItems(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        if (LethalContent.Items.IsFrozen)
        {
            orig(self);
            return;
        }

        foreach (DawnItemInfo itemInfo in LethalContent.Items.Values)
        {
            if (itemInfo.ShouldSkipIgnoreOverride() || self.allItemsList.itemsList.Contains(itemInfo.Item))
                continue;

            self.allItemsList.itemsList.Add(itemInfo.Item);
        }
        orig(self);
    }

    private static void RedoItemsDebugMenu()
    {
        QuickMenuManagerRefs.Instance.Debug_SetAllItemsDropdownOptions();
    }

    internal static void UpdateAllShopItemPrices()
    {
        foreach (DawnItemInfo itemInfo in LethalContent.Items.Values)
        {
            DawnShopItemInfo? shopInfo = itemInfo.ShopInfo;
            if (shopInfo == null || (itemInfo.ShouldSkipRespectOverride()))
                continue;

            UpdateShopItemPrices(shopInfo);
        }
    }

    static void UpdateShopItemPrices(DawnShopItemInfo shopInfo)
    {
        int cost = shopInfo.DawnPurchaseInfo.Cost.Provide();
        shopInfo.ParentInfo.Item.creditsWorth = cost;
        shopInfo.ReceiptNode.itemCost = cost;
        shopInfo.RequestNode.itemCost = cost;
    }

    private static void UpdateItemWeights(On.RoundManager.orig_SpawnScrapInLevel orig, RoundManager self)
    {
        UpdateItemWeightsOnLevel(self.currentLevel);
        List<SpawnableItemWithRarity> zeroWeightItems = new();
        foreach (SpawnableItemWithRarity spawnableItemWithRarity in self.currentLevel.spawnableScrap.ToArray())
        {
            if (spawnableItemWithRarity.rarity <= 0)
            {
                zeroWeightItems.Add(spawnableItemWithRarity);
                self.currentLevel.spawnableScrap.Remove(spawnableItemWithRarity);
            }
        }
        orig(self);
        foreach (SpawnableItemWithRarity spawnableItemWithRarity in zeroWeightItems)
        {
            self.currentLevel.spawnableScrap.Add(spawnableItemWithRarity);
        }
    }

    private static void UpdateItemWeights(On.StartOfRound.orig_SetPlanetsWeather orig, StartOfRound self, int connectedPlayersOnServer)
    {
        orig(self, connectedPlayersOnServer);
        UpdateItemWeightsOnLevel(self.currentLevel);
    }

    internal static void UpdateItemWeightsOnLevel(SelectableLevel level)
    {
        if (!LethalContent.Weathers.IsFrozen || !LethalContent.Items.IsFrozen || StartOfRound.Instance == null || (WeatherRegistryCompat.Enabled && !WeatherRegistryCompat.IsWeatherManagerReady()))
            return;

        foreach (DawnItemInfo itemInfo in LethalContent.Items.Values)
        {
            DawnScrapItemInfo? scrapInfo = itemInfo.ScrapInfo;
            if (scrapInfo == null || (itemInfo.ShouldSkipRespectOverride()))
                continue;

            Debuggers.Items?.Log($"Updating {itemInfo.Item.itemName}'s weights on level {level.PlanetName}.");
            SpawnableItemWithRarity? spawnableItemWithRarity = level.spawnableScrap.FirstOrDefault(x => x.spawnableItem == itemInfo.Item);
            if (spawnableItemWithRarity == null)
            {
                spawnableItemWithRarity = new()
                {
                    spawnableItem = itemInfo.Item,
                    rarity = 0
                };
                level.spawnableScrap.Add(spawnableItemWithRarity);
            }
            spawnableItemWithRarity.rarity = scrapInfo.Weights.GetFor(level.GetDawnInfo()) ?? 0;
        }
    }

    private static void FreezeItemContent()
    {
        Dictionary<string, WeightTableBuilder<DawnMoonInfo>> itemWeightBuilder = new();
        Dictionary<string, DawnShopItemInfo> itemsWithShopInfo = new();
        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            SelectableLevel level = moonInfo.Level;

            foreach (SpawnableItemWithRarity itemWithRarity in level.spawnableScrap)
            {
                if (!itemWeightBuilder.TryGetValue(itemWithRarity.spawnableItem.name, out WeightTableBuilder<DawnMoonInfo> weightTableBuilder))
                {
                    weightTableBuilder = new WeightTableBuilder<DawnMoonInfo>();
                    itemWeightBuilder[itemWithRarity.spawnableItem.name] = weightTableBuilder;
                }
                Debuggers.Items?.Log($"Adding weight {itemWithRarity.rarity} to {itemWithRarity.spawnableItem.name} on level {level.PlanetName}");
                weightTableBuilder.AddWeight(moonInfo.TypedKey, itemWithRarity.rarity);
            }
        }

        Terminal terminal = TerminalRefs.Instance;
        TerminalKeyword buyKeyword = TerminalRefs.BuyKeyword;
        TerminalKeyword infoKeyword = TerminalRefs.InfoKeyword;

        List<CompatibleNoun> buyCompatibleNouns = buyKeyword.compatibleNouns.ToList();
        List<CompatibleNoun> infoCompatibleNouns = infoKeyword.compatibleNouns.ToList();
        List<TerminalKeyword> terminalKeywords = terminal.terminalNodes.allKeywords.ToList();

        for (int i = 0; i < terminal.buyableItemsList.Length; i++)
        {
            Item buyableItem = terminal.buyableItemsList[i];
            TerminalNode? infoNode = null;
            TerminalNode? requestNode = null;
            TerminalNode receiptNode = null!;

            string simplifiedItemName = buyableItem.itemName.Replace(" ", "-").ToLowerInvariant();

            requestNode = TerminalRefs.BuyKeyword.compatibleNouns.Where(noun => noun.result.buyItemIndex == i).Select(noun => noun.result).FirstOrDefault();
            if (requestNode == null)
            {
                DawnPlugin.Logger.LogWarning($"No request Node found for {buyableItem.itemName} despite it being a buyable item, this is likely the result of an item being removed from the shop items list by LethalLib, i.e. Night Vision Goggles from MoreShipUpgrades");
                continue;
            }
            receiptNode = requestNode.terminalOptions[0].result;
            TerminalKeyword? buyKeywordOfSignificance = TerminalRefs.BuyKeyword.compatibleNouns.Where(noun => noun.result == requestNode).Select(noun => noun.noun).FirstOrDefault();
            if (buyKeywordOfSignificance == null)
            {
                DawnPlugin.Logger.LogWarning($"No buy Keyword found for {buyableItem.itemName} despite it being a buyable item, this is likely the result of an item being removed from the shop items list by LethalLib, i.e. Night Vision Goggles from MoreShipUpgrades");
                continue;
            }

            foreach (CompatibleNoun compatibleNouns in infoKeyword.compatibleNouns)
            {
                if (compatibleNouns.noun == buyKeywordOfSignificance)
                {
                    infoNode = compatibleNouns.result;
                    break;
                }
            }

            if (infoNode == null)
            {
                infoNode = new TerminalNodeBuilder($"{simplifiedItemName}InfoNode")
                    .SetDisplayText($"[No information about this object was found.]\n\n")
                    .SetClearPreviousText(true)
                    .SetMaxCharactersToType(25)
                    .Build();

                CompatibleNoun compatibleNounMissing = new()
                {
                    noun = buyKeywordOfSignificance,
                    result = infoNode
                };
                List<CompatibleNoun> newCompatibleNouns = infoKeyword.compatibleNouns.ToList();
                newCompatibleNouns.Add(compatibleNounMissing);
                infoKeyword.compatibleNouns = newCompatibleNouns.ToArray();
            }

            foreach (var compatibleNouns in buyKeyword.compatibleNouns)
            {
                if (compatibleNouns.noun == buyKeywordOfSignificance)
                {
                    requestNode = compatibleNouns.result;
                    break;
                }
            }

            DawnPurchaseInfo purchaseInfo = new(new SimpleProvider<int>(buyableItem.creditsWorth), ITerminalPurchasePredicate.AlwaysSuccess());
            DawnShopItemInfo shopInfo = new(purchaseInfo, infoNode, requestNode, receiptNode);
            itemsWithShopInfo[buyableItem.name] = shopInfo;
        }

        foreach (Item item in StartOfRound.Instance.allItemsList.itemsList)
        {
            if (item.HasDawnInfo())
                continue;

            string name = NamespacedKey.NormalizeStringForNamespacedKey(item.itemName, true);
            NamespacedKey<DawnItemInfo>? key = ItemKeys.GetByReflection(name);
            if (key == null && LethalLibCompat.Enabled && LethalLibCompat.TryGetItemFromLethalLib(item, out string lethalLibModName))
            {
                key = NamespacedKey<DawnItemInfo>.From(NamespacedKey.NormalizeStringForNamespacedKey(lethalLibModName, false), NamespacedKey.NormalizeStringForNamespacedKey(item.itemName, false));
            }
            else if (key == null && LethalLevelLoaderCompat.Enabled && LethalLevelLoaderCompat.TryGetExtendedItemModName(item, out string lethalLevelLoaderModName))
            {
                key = NamespacedKey<DawnItemInfo>.From(NamespacedKey.NormalizeStringForNamespacedKey(lethalLevelLoaderModName, false), NamespacedKey.NormalizeStringForNamespacedKey(item.itemName, false));
            }
            else if (key == null)
            {
                key = NamespacedKey<DawnItemInfo>.From("unknown_lib", NamespacedKey.NormalizeStringForNamespacedKey(item.itemName, false));
            }

            if (LethalContent.Items.ContainsKey(key))
            {
                DawnPlugin.Logger.LogWarning($"Item {item.itemName} is already registered by the same creator to LethalContent. This is likely to cause issues.");
                item.SetDawnInfo(LethalContent.Items[key]);
                continue;
            }
            itemWeightBuilder.TryGetValue(item.name, out WeightTableBuilder<DawnMoonInfo>? weightTableBuilder);
            DawnScrapItemInfo? scrapInfo = null;
            itemsWithShopInfo.TryGetValue(item.name, out DawnShopItemInfo? shopInfo);

            if (weightTableBuilder != null)
            {
                scrapInfo = new(weightTableBuilder.Build());
            }

            if (!item.spawnPrefab)
            {
                DawnPlugin.Logger.LogWarning($"{item.itemName} ({item.name}) didn't have a spawn prefab?");
                continue;
            }

            Debuggers.Items?.Log($"Registering {item.itemName} ({item.name}) with range of values: {item.minValue * 0.4} and {item.maxValue * 0.4}");
            HashSet<NamespacedKey> tags = [DawnLibTags.IsExternal];
            CollectLLLTags(item, tags);
            DawnItemInfo itemInfo = new(key, tags, item, scrapInfo, shopInfo, null);
            item.SetDawnInfo(itemInfo);
            LethalContent.Items.Register(itemInfo);
        }

        RegisterScrapItemsToAllLevels();
        LethalContent.Items.Freeze();
    }

    private static void RegisterScrapItemsToAllLevels()
    {
        foreach (DawnItemInfo itemInfo in LethalContent.Items.Values)
        {
            DawnScrapItemInfo? scrapInfo = itemInfo.ScrapInfo;
            if (scrapInfo == null || (itemInfo.ShouldSkipRespectOverride()))
                continue;

            foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
            {
                SelectableLevel level = moonInfo.Level;
                bool alreadyExists = false;
                foreach (SpawnableItemWithRarity newSpawnableItemWithRarity in level.spawnableScrap)
                {
                    if (newSpawnableItemWithRarity.spawnableItem == itemInfo.Item)
                    {
                        alreadyExists = true;
                        break;
                    }
                }

                if (alreadyExists)
                    continue;

                SpawnableItemWithRarity spawnDef = new()
                {
                    spawnableItem = itemInfo.Item,
                    rarity = 0
                };
                level.spawnableScrap.Add(spawnDef);
            }
        }
    }

    private static void RegisterShopItemsToTerminal(On.Terminal.orig_Awake orig, Terminal self)
    {
        orig(self);
        foreach (DawnItemInfo itemInfo in LethalContent.Items.Values)
        {
            TryRegisterItemIntoShop(itemInfo.Item);
        }
    }

    private static void TryRegisterItemIntoShop(Item item)
    {
        DawnItemInfo itemInfo = item.GetDawnInfo();
        if (itemInfo == null)
        {
            DawnPlugin.Logger.LogWarning($"Item: {item.itemName} does not have a dawn info, this means they cannot be registered to the terminal as a shop item");
            return;
        }

        _ = TerminalRefs.Instance;
        TerminalKeyword buyKeyword = TerminalRefs.BuyKeyword;
        TerminalKeyword infoKeyword = TerminalRefs.InfoKeyword;
        TerminalKeyword confirmPurchaseKeyword = TerminalRefs.ConfirmPurchaseKeyword;
        TerminalKeyword denyPurchaseKeyword = TerminalRefs.DenyKeyword;
        TerminalNode cancelPurchaseNode = TerminalRefs.CancelPurchaseNode;

        DawnShopItemInfo? shopInfo = itemInfo.ShopInfo;
        if (shopInfo == null || itemInfo.ShouldSkipRespectOverride() || TerminalRefs.Instance.buyableItemsList.Contains(item))
            return;

        List<Item> newBuyableList = TerminalRefs.Instance.buyableItemsList.ToList();

        string simplifiedItemName = itemInfo.Item.itemName.Replace(" ", "-").ToLowerInvariant();

        newBuyableList.Add(itemInfo.Item);

        foreach (TerminalKeyword TerminalKeyword in TerminalRefs.Instance.terminalNodes.allKeywords)
        {
            if (TerminalKeyword.name == simplifiedItemName)
            {
                TerminalRefs.Instance.buyableItemsList = newBuyableList.ToArray(); // this needs to be restored on lobby reload
                return;
            }
        }

        List<CompatibleNoun> newBuyCompatibleNouns = buyKeyword.compatibleNouns.ToList();
        List<CompatibleNoun> newInfoCompatibleNouns = infoKeyword.compatibleNouns.ToList();
        List<TerminalKeyword> newTerminalKeywords = TerminalRefs.Instance.terminalNodes.allKeywords.ToList();

        TerminalKeyword buyItemKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();
        buyItemKeyword.name = simplifiedItemName;
        buyItemKeyword.word = simplifiedItemName;
        buyItemKeyword.isVerb = false;
        buyItemKeyword.compatibleNouns = null;
        buyItemKeyword.specialKeywordResult = null;
        buyItemKeyword.defaultVerb = buyKeyword;
        buyItemKeyword.accessTerminalObjects = false;
        newTerminalKeywords.Add(buyItemKeyword);

        TerminalNode receiptNode = shopInfo.ReceiptNode;
        TerminalNode requestNode = shopInfo.RequestNode;

        UpdateShopItemPrices(shopInfo);
        receiptNode.buyItemIndex = newBuyableList.Count - 1;

        requestNode.buyItemIndex = newBuyableList.Count - 1;
        requestNode.isConfirmationNode = true;
        requestNode.overrideOptions = true;
        requestNode.terminalOptions =
        [
            new CompatibleNoun()
            {
                noun = confirmPurchaseKeyword,
                result = receiptNode
            },
            new CompatibleNoun()
            {
                noun = denyPurchaseKeyword,
                result = cancelPurchaseNode
            }
        ];

        newBuyCompatibleNouns.Add(new CompatibleNoun()
        {
            noun = buyItemKeyword,
            result = requestNode
        });
        newInfoCompatibleNouns.Add(new CompatibleNoun()
        {
            noun = buyItemKeyword,
            result = shopInfo.InfoNode
        });

        TerminalRefs.Instance.buyableItemsList = newBuyableList.ToArray(); // this needs to be restored on lobby reload
        infoKeyword.compatibleNouns = newInfoCompatibleNouns.ToArray(); // SO so it sticks
        buyKeyword.compatibleNouns = newBuyCompatibleNouns.ToArray(); // SO so it sticks
        TerminalRefs.Instance.terminalNodes.allKeywords = newTerminalKeywords.ToArray(); // SO so it sticks
    }

    private static void CollectLLLTags(Item item, HashSet<NamespacedKey> tags)
    {
        if (LethalLevelLoaderCompat.Enabled && LethalLevelLoaderCompat.TryGetAllTagsWithModNames(item, out List<(string modName, string tagName)> tagsWithModNames))
        {
            tags.AddToList(tagsWithModNames, Debuggers.Items, item.name);
        }
    }
}