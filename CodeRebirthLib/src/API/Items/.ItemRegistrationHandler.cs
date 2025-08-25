using System.Collections.Generic;
using System.Linq;
using CodeRebirthLib.Internal;
using CodeRebirthLib.Internal.ModCompats;
using UnityEngine;

namespace CodeRebirthLib;

static class ItemRegistrationHandler
{
    internal static void Init()
    {
        On.RoundManager.SpawnScrapInLevel += UpdateItemWeights;
        On.StartOfRound.SetPlanetsWeather += UpdateItemWeights;
        On.Terminal.Awake += RegisterShopItemsToTerminal;
        LethalContent.Moons.OnFreeze += FreezeItemContent;
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
        if (!LethalContent.Items.IsFrozen)
            return;

        foreach (CRItemInfo itemInfo in LethalContent.Items.Values)
        {
            CRScrapItemInfo? scrapInfo = itemInfo.ScrapInfo;
            if (scrapInfo == null || itemInfo.Key.IsVanilla() || itemInfo.HasTag(CRLibTags.IsExternal))
                continue;

            Debuggers.Items?.Log($"Updating {itemInfo.Item.itemName}'s weights on level {level.PlanetName}.");
            level.spawnableScrap.Where(x => x.spawnableItem == itemInfo.Item).First().rarity = scrapInfo.Weights.GetFor(LethalContent.Moons[level.ToNamespacedKey()]) ?? 0;
        }
    }

    private static void FreezeItemContent()
    {
        if (LethalContent.Items.IsFrozen)
            return;

        Dictionary<Item, WeightTableBuilder<CRMoonInfo>> itemWeightBuilder = new();
        Dictionary<Item, CRShopItemInfo> itemsWithShopInfo = new();
        foreach (CRMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            SelectableLevel level = moonInfo.Level;

            foreach (var itemWithRarity in level.spawnableScrap)
            {
                if (!itemWeightBuilder.TryGetValue(itemWithRarity.spawnableItem, out WeightTableBuilder<CRMoonInfo> weightTableBuilder))
                {
                    weightTableBuilder = new WeightTableBuilder<CRMoonInfo>();
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
            CRShopItemInfo shopInfo = new(new AlwaysAvaliableTerminalPredicate(), infoNode, requestNode, receiptNode, buyableItem.creditsWorth);
            itemsWithShopInfo[buyableItem] = shopInfo;
        }

        foreach (Item item in StartOfRound.Instance.allItemsList.itemsList)
        {
            if (item.TryGetCRInfo(out _))
                continue;

            NamespacedKey<CRItemInfo>? key = (NamespacedKey<CRItemInfo>?)typeof(ItemKeys).GetField(NamespacedKey.NormalizeStringForNamespacedKey(item.itemName, true))?.GetValue(null);
            key ??= NamespacedKey<CRItemInfo>.From("modded_please_replace_this_later", NamespacedKey.NormalizeStringForNamespacedKey(item.itemName, false));

            itemWeightBuilder.TryGetValue(item, out WeightTableBuilder<CRMoonInfo>? weightTableBuilder);
            CRScrapItemInfo? scrapInfo = null;
            itemsWithShopInfo.TryGetValue(item, out CRShopItemInfo? shopInfo);

            if (weightTableBuilder != null)
            {
                scrapInfo = new(weightTableBuilder.Build());
            }

            List<NamespacedKey> tags = [CRLibTags.IsExternal];
            if (item.spawnPrefab.GetComponent<GrabbableObject>().GetType() == typeof(GrabbableObject))
            {
                tags.Add(Tags.NonInteractable);
            }
            else if (item.spawnPrefab.GetComponent<NoisemakerProp>() != null)
            {
                tags.Add(Tags.Noisy);
            }
            else
            {
                tags.Add(Tags.Interactable);
            }

            if (shopInfo != null)
            {
                tags.Add(Tags.Buyable);
            }

            if (scrapInfo != null)
            {
                tags.Add(Tags.Scrap);
            }

            if (item.requiresBattery)
            {
                tags.Add(Tags.Chargeable);
            }

            if (item.isConductiveMetal)
            {
                tags.Add(Tags.Conductive);
            }

            // do the weights and values

            if (item.twoHanded)
            {
                tags.Add(Tags.TwoHanded);
            }
            else
            {
                tags.Add(Tags.OneHanded);
            }

            if (item.isDefensiveWeapon)
            {
                tags.Add(Tags.Weapon);
            }

            if (LLLCompat.Enabled && LLLCompat.TryGetAllTagsWithModNames(item, out List<(string modName, string tagName)> tagsWithModNames))
            {
                foreach ((string modName, string tagName) in tagsWithModNames)
                {
                    bool alreadyAdded = false;
                    foreach (NamespacedKey tag in tags)
                    {
                        if (tag.Key == tagName)
                        {
                            alreadyAdded = true;
                            break;
                        }
                    }

                    if (alreadyAdded)
                        continue;

                    string normalizedModName = NamespacedKey.NormalizeStringForNamespacedKey(modName, false);
                    string normalizedTagName = NamespacedKey.NormalizeStringForNamespacedKey(tagName, false);
                    Debuggers.Items?.Log($"Adding tag {normalizedModName}:{normalizedTagName} to item {item.itemName}");
                    tags.Add(NamespacedKey.From(normalizedModName, normalizedTagName));
                }
            }
            CRItemInfo itemInfo = new(key, tags, item, scrapInfo, shopInfo);
            item.SetCRInfo(itemInfo);
            LethalContent.Items.Register(itemInfo);
        }

        RegisterScrapItemsToAllLevels();
        LethalContent.Items.Freeze();
    }

    private static void RegisterScrapItemsToAllLevels()
    {
        foreach (CRItemInfo itemInfo in LethalContent.Items.Values)
        {
            CRScrapItemInfo? scrapInfo = itemInfo.ScrapInfo;
            if (scrapInfo == null || itemInfo.Key.IsVanilla() || itemInfo.HasTag(CRLibTags.IsExternal))
                continue;

            foreach (CRMoonInfo moonInfo in LethalContent.Moons.Values)
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

        foreach (CRItemInfo itemInfo in LethalContent.Items.Values)
        {
            CRShopItemInfo? shopInfo = itemInfo.ShopInfo;
            if (shopInfo == null || itemInfo.Key.IsVanilla() || itemInfo.HasTag(CRLibTags.IsExternal))
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

            receiptNode.buyItemIndex = newBuyableList.Count - 1;
            receiptNode.itemCost = shopInfo.Cost;

            requestNode.buyItemIndex = newBuyableList.Count - 1;
            requestNode.isConfirmationNode = true;
            requestNode.overrideOptions = true;
            requestNode.itemCost = shopInfo.Cost;
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