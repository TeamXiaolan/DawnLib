using System;
using System.Collections.Generic;
using System.Linq;
using Dawn.Internal;
using Dawn.Utils;
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

        On.RoundManager.SpawnScrapInLevel += UpdateItemWeights;
        On.StartOfRound.SetPlanetsWeather += UpdateItemWeights;
        On.Terminal.Awake += RegisterShopItemsToTerminal;
        LethalContent.Moons.OnFreeze += FreezeItemContent;
        LethalContent.Items.OnFreeze += RedoItemsDebugMenu;
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
            if (shopInfo == null || itemInfo.HasTag(DawnLibTags.IsExternal))
                continue;

            UpdateShopItemPrices(shopInfo);
        }
    }

    static void UpdateShopItemPrices(DawnShopItemInfo shopInfo)
    {
        int cost = shopInfo.Cost.Provide();
        shopInfo.ParentInfo.Item.creditsWorth = cost;
        shopInfo.ReceiptNode.itemCost = cost;
        shopInfo.RequestNode.itemCost = cost;
    }

    private static void UpdateItemWeights(On.RoundManager.orig_SpawnScrapInLevel orig, RoundManager self)
    {
        UpdateItemWeightsOnLevel(self.currentLevel);
        orig(self);
    }

    private static void UpdateItemWeights(On.StartOfRound.orig_SetPlanetsWeather orig, StartOfRound self, int connectedPlayersOnServer)
    {
        orig(self, connectedPlayersOnServer);
        UpdateItemWeightsOnLevel(self.currentLevel);
    }

    internal static void UpdateItemWeightsOnLevel(SelectableLevel level)
    {
        if (!LethalContent.Items.IsFrozen || StartOfRound.Instance == null || (WeatherRegistryCompat.Enabled && WeatherRegistryCompat.IsWeatherManagerReady()))
            return;

        foreach (DawnItemInfo itemInfo in LethalContent.Items.Values)
        {
            DawnScrapItemInfo? scrapInfo = itemInfo.ScrapInfo;
            if (scrapInfo == null || itemInfo.HasTag(DawnLibTags.IsExternal))
                continue;

            Debuggers.Items?.Log($"Updating {itemInfo.Item.itemName}'s weights on level {level.PlanetName}.");
            level.spawnableScrap.Where(x => x.spawnableItem == itemInfo.Item).First().rarity = scrapInfo.Weights.GetFor(level.GetDawnInfo()) ?? 0;
        }
    }

    private static void FreezeItemContent()
    {
        if (LethalContent.Items.IsFrozen)
            return;

        Dictionary<Item, WeightTableBuilder<DawnMoonInfo>> itemWeightBuilder = new();
        Dictionary<Item, DawnShopItemInfo> itemsWithShopInfo = new();
        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            SelectableLevel level = moonInfo.Level;

            foreach (var itemWithRarity in level.spawnableScrap)
            {
                if (!itemWeightBuilder.TryGetValue(itemWithRarity.spawnableItem, out WeightTableBuilder<DawnMoonInfo> weightTableBuilder))
                {
                    weightTableBuilder = new WeightTableBuilder<DawnMoonInfo>();
                    itemWeightBuilder[itemWithRarity.spawnableItem] = weightTableBuilder;
                }
                weightTableBuilder.AddWeight(moonInfo.TypedKey, itemWithRarity.rarity);
            }
        }

        Terminal terminal = TerminalRefs.Instance;
        TerminalKeyword buyKeyword = TerminalRefs.BuyKeyword;
        TerminalKeyword infoKeyword = TerminalRefs.InfoKeyword;

        List<CompatibleNoun> buyCompatibleNouns = buyKeyword.compatibleNouns.ToList();
        List<CompatibleNoun> infoCompatibleNouns = infoKeyword.compatibleNouns.ToList();
        List<TerminalKeyword> terminalKeywords = terminal.terminalNodes.allKeywords.ToList();

        foreach (var terminalKeyword in terminalKeywords)
        {
            Debuggers.Items?.Log($"TerminalKeyword.word for {terminalKeyword.name}: {terminalKeyword.word}");
            terminalKeyword.word = terminalKeyword.word.Replace(" ", "-").ToLowerInvariant();
        }

        for (int i = 0; i < terminal.buyableItemsList.Length; i++)
        {
            Item buyableItem = terminal.buyableItemsList[i];
            TerminalNode? infoNode = null;
            TerminalNode? requestNode = null;
            TerminalNode receiptNode = null!;

            Debuggers.Items?.Log($"Processing {buyableItem.itemName}");
            string simplifiedItemName = buyableItem.itemName.Replace(" ", "-").ToLowerInvariant();

            requestNode = TerminalRefs.BuyKeyword.compatibleNouns.Where(noun => noun.result.buyItemIndex == i).Select(noun => noun.result).FirstOrDefault();
            if (requestNode == null)
            {
                DawnPlugin.Logger.LogWarning($"No request Node found for {buyableItem.itemName} despite it being a buyable item, this is likely the result of an item being removed from the shop items list by LethalLib, i.e. Night Vision Goggles from MoreShipUpgrades");
                continue;
            }
            receiptNode = requestNode.terminalOptions[0].result;
            TerminalKeyword? buyKeywordOfSignificance = TerminalRefs.BuyKeyword.compatibleNouns.Where(noun => noun.result == requestNode).Select(noun => noun.noun).FirstOrDefault() ?? terminalKeywords.FirstOrDefault(keyword => keyword.word == simplifiedItemName);
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
                Debuggers.Items?.Log($"Checking compatible nouns for info node: {compatibleNouns.noun.word}");
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
                Debuggers.Items?.Log($"Checking compatible nouns for request node: {compatibleNouns.noun.word}");
            }

            DawnShopItemInfo shopInfo = new(ITerminalPurchasePredicate.AlwaysSuccess(), infoNode, requestNode, receiptNode, new SimpleProvider<int>(buyableItem.creditsWorth));
            itemsWithShopInfo[buyableItem] = shopInfo;
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
            itemWeightBuilder.TryGetValue(item, out WeightTableBuilder<DawnMoonInfo>? weightTableBuilder);
            DawnScrapItemInfo? scrapInfo = null;
            itemsWithShopInfo.TryGetValue(item, out DawnShopItemInfo? shopInfo);

            if (weightTableBuilder != null)
            {
                scrapInfo = new(weightTableBuilder.Build());
            }

            if (!item.spawnPrefab)
            {
                DawnPlugin.Logger.LogWarning($"{item.itemName} ({item.name}) didn't have a spawn prefab?");
                continue;
            }

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
            if (scrapInfo == null || itemInfo.HasTag(DawnLibTags.IsExternal))
                continue;

            foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
            {
                SelectableLevel level = moonInfo.Level;
                SpawnableItemWithRarity spawnDef = new()
                {
                    spawnableItem = itemInfo.Item,
                    rarity = 0
                };
                level.spawnableScrap.Add(spawnDef);
            }

            if (itemInfo.ShopInfo != null)
                continue;

            StartOfRound.Instance.allItemsList.itemsList.Add(itemInfo.Item);
        }
    }

    private static void RegisterShopItemsToTerminal(On.Terminal.orig_Awake orig, Terminal self)
    {
        Terminal terminal = TerminalRefs.Instance;
        TerminalKeyword buyKeyword = TerminalRefs.BuyKeyword;
        TerminalKeyword infoKeyword = TerminalRefs.InfoKeyword;
        TerminalKeyword confirmPurchaseKeyword = TerminalRefs.ConfirmPurchaseKeyword;
        TerminalKeyword denyPurchaseKeyword = TerminalRefs.DenyKeyword;
        TerminalNode cancelPurchaseNode = TerminalRefs.CancelPurchaseNode;

        List<Item> newBuyableList = self.buyableItemsList.ToList();
        List<CompatibleNoun> newBuyCompatibleNouns = buyKeyword.compatibleNouns.ToList();
        List<CompatibleNoun> newInfoCompatibleNouns = infoKeyword.compatibleNouns.ToList();
        List<TerminalKeyword> newTerminalKeywords = self.terminalNodes.allKeywords.ToList();

        foreach (DawnItemInfo itemInfo in LethalContent.Items.Values)
        {
            DawnShopItemInfo? shopInfo = itemInfo.ShopInfo;
            if (shopInfo == null || itemInfo.HasTag(DawnLibTags.IsExternal))
                continue;

            string simplifiedItemName = itemInfo.Item.itemName.Replace(" ", "-").ToLowerInvariant();

            newBuyableList.Add(itemInfo.Item);

            foreach (var TerminalKeyword in newTerminalKeywords)
            {
                if (TerminalKeyword.word == simplifiedItemName)
                {
                    continue;
                }
            }

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
            StartOfRoundRefs.Instance.allItemsList.itemsList.Add(itemInfo.Item);
        }

        self.buyableItemsList = newBuyableList.ToArray(); // this needs to be restored on lobby reload
        infoKeyword.compatibleNouns = newInfoCompatibleNouns.ToArray(); // SO so it sticks
        buyKeyword.compatibleNouns = newBuyCompatibleNouns.ToArray(); // SO so it sticks
        self.terminalNodes.allKeywords = newTerminalKeywords.ToArray(); // SO so it sticks
        orig(self);
    }

    private static void CollectLLLTags(Item item, HashSet<NamespacedKey> tags)
    {
        if (LethalLevelLoaderCompat.Enabled && LethalLevelLoaderCompat.TryGetAllTagsWithModNames(item, out List<(string modName, string tagName)> tagsWithModNames))
        {
            tags.AddToList(tagsWithModNames, Debuggers.Items, item.name);
        }
    }
}