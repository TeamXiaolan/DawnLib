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
    }

    internal static void UpdateAllShopItemPrices()
    {
        foreach (DawnItemInfo itemInfo in LethalContent.Items.Values)
        {
            DawnShopItemInfo? shopInfo = itemInfo.ShopInfo;
            if(shopInfo == null || itemInfo.HasTag(DawnLibTags.IsExternal))
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
            level.spawnableScrap.Where(x => x.spawnableItem == itemInfo.Item).First().rarity = scrapInfo.Weights.GetFor(LethalContent.Moons[level.ToNamespacedKey()]) ?? 0;
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

        Terminal terminal = GameObject.FindFirstObjectByType<Terminal>();
        TerminalKeyword buyKeyword = terminal.terminalNodes.allKeywords.First(keyword => keyword.word == "buy");
        TerminalKeyword infoKeyword = terminal.terminalNodes.allKeywords.First(keyword => keyword.word == "info");

        List<CompatibleNoun> buyCompatibleNouns = buyKeyword.compatibleNouns.ToList();
        List<CompatibleNoun> infoCompatibleNouns = infoKeyword.compatibleNouns.ToList();
        List<TerminalKeyword> terminalKeywords = terminal.terminalNodes.allKeywords.ToList();

        foreach (var terminalKeyword in terminalKeywords)
        {
            Debuggers.Items?.Log($"TerminalKeyword.word for {terminalKeyword.name}: {terminalKeyword.word}");
            terminalKeyword.word = terminalKeyword.word.Replace(" ", "-").ToLowerInvariant();
        }

        foreach (var buyableItem in terminal.buyableItemsList)
        {
            TerminalNode? infoNode = null;
            TerminalNode requestNode = null!;
            TerminalNode receiptNode = null!;

            Debuggers.Items?.Log($"Processing {buyableItem.itemName}");
            string simplifiedItemName = buyableItem.itemName.Replace(" ", "-").ToLowerInvariant();
            if (simplifiedItemName.Equals("stun-grenade"))
            {
                simplifiedItemName = "stun";
            }
            else if (simplifiedItemName.Equals("tzp-inhalant"))
            {
                simplifiedItemName = "tzp";
            }
            else if (simplifiedItemName.Equals("radar-booster"))
            {
                simplifiedItemName = "radar";
            }
            TerminalKeyword buyKeywordOfSignificance = terminalKeywords.First(keyword => keyword.word == simplifiedItemName);

            foreach (var compatibleNouns in infoKeyword.compatibleNouns)
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

            receiptNode = requestNode.terminalOptions[0].result;
            DawnShopItemInfo shopInfo = new(ITerminalPurchasePredicate.AlwaysSuccess(), infoNode, requestNode, receiptNode, new SimpleProvider<int>(buyableItem.creditsWorth));
            itemsWithShopInfo[buyableItem] = shopInfo;
        }

        foreach (Item item in StartOfRound.Instance.allItemsList.itemsList)
        {
            if (item.TryGetDawnInfo(out _))
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
                DawnPlugin.Logger.LogWarning($"Item {item.itemName} is already registered by the same creator to LethalContent. Skipping...");
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

            List<NamespacedKey> tags = [DawnLibTags.IsExternal];
            if (LethalLevelLoaderCompat.Enabled && LethalLevelLoaderCompat.TryGetAllTagsWithModNames(item, out List<(string modName, string tagName)> tagsWithModNames))
            {
                foreach ((string modName, string tagName) in tagsWithModNames)
                {
                    string normalizedModName = NamespacedKey.NormalizeStringForNamespacedKey(modName, false);
                    string normalizedTagName = NamespacedKey.NormalizeStringForNamespacedKey(tagName, false);

                    if (normalizedModName == "lethalcompany")
                    {
                        normalizedModName = "lethal_level_loader";
                    }
                    Debuggers.Items?.Log($"Adding tag {normalizedModName}:{normalizedTagName} to item {item.itemName}");
                    tags.Add(NamespacedKey.From(normalizedModName, normalizedTagName));
                }
            }
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
        TerminalKeyword buyKeyword = self.terminalNodes.allKeywords.First(keyword => keyword.word == "buy");
        TerminalKeyword infoKeyword = self.terminalNodes.allKeywords.First(keyword => keyword.word == "info");
        TerminalKeyword confirmPurchaseKeyword = self.terminalNodes.allKeywords.First(keyword2 => keyword2.word == "confirm");
        TerminalKeyword denyPurchaseKeyword = self.terminalNodes.allKeywords.First(keyword2 => keyword2.word == "deny");
        TerminalNode cancelPurchaseNode = buyKeyword.compatibleNouns[0].result.terminalOptions[1].result;

        List<Item> newBuyableList = self.buyableItemsList.ToList();
        List<CompatibleNoun> newBuyCompatibleNouns = buyKeyword.compatibleNouns.ToList();
        List<CompatibleNoun> newInfoCompatibleNouns = infoKeyword.compatibleNouns.ToList();
        List<TerminalKeyword> newTerminalKeywords = self.terminalNodes.allKeywords.ToList();
        StartOfRound startOfRound = GameObject.FindFirstObjectByType<StartOfRound>();

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
            startOfRound.allItemsList.itemsList.Add(itemInfo.Item);
        }

        self.buyableItemsList = newBuyableList.ToArray(); // this needs to be restored on lobby reload
        infoKeyword.compatibleNouns = newInfoCompatibleNouns.ToArray(); // SO so it sticks
        buyKeyword.compatibleNouns = newBuyCompatibleNouns.ToArray(); // SO so it sticks
        self.terminalNodes.allKeywords = newTerminalKeywords.ToArray(); // SO so it sticks
        orig(self);
    }
}