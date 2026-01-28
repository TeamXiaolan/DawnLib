using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dawn.Internal;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace Dawn;

[HarmonyPatch]
static class UnlockableRegistrationHandler
{
    internal static void Init()
    {
        using (new DetourContext(priority: -100))
        {
            On.StartOfRound.Awake += FixRegularUnlockables;
            On.Terminal.Awake += RegisterDawnUnlockables;
        }
        On.Terminal.TextPostProcess += AddShipUpgradesToTerminal;
    }

    private static void FixRegularUnlockables(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        orig(self);
        int roomLayer = LayerMask.NameToLayer("Room");
        // Table
        StartOfRoundRefs.Instance.unlockablesList.unlockables[13].prefabObject.transform.Find("TableMesh").gameObject.layer = roomLayer;
        // Romantic Table
        StartOfRoundRefs.Instance.unlockablesList.unlockables[14].prefabObject.transform.Find("RTableMesh").gameObject.layer = roomLayer;
        // Signal Translator
        StartOfRoundRefs.Instance.unlockablesList.unlockables[17].prefabObject.transform.Find("Mesh").gameObject.layer = roomLayer;
        StartOfRoundRefs.Instance.unlockablesList.unlockables[17].prefabObject.transform.Find("Mesh").gameObject.AddComponent<BoxCollider>().size = new Vector3(0.83f, 0.31f, 0.31f);
        // Microwave
        StartOfRoundRefs.Instance.unlockablesList.unlockables[28].prefabObject.transform.Find("MicrowaveBody").gameObject.layer = roomLayer;
        // Sofa
        StartOfRoundRefs.Instance.unlockablesList.unlockables[29].prefabObject.transform.Find("SofaChairMesh/Cube").gameObject.layer = roomLayer;
        StartOfRoundRefs.Instance.unlockablesList.unlockables[29].prefabObject.transform.Find("SofaChairMesh/Cube (1)").gameObject.layer = roomLayer;
        StartOfRoundRefs.Instance.unlockablesList.unlockables[29].prefabObject.transform.Find("SofaChairMesh/Cube (2)").gameObject.layer = roomLayer;
        StartOfRoundRefs.Instance.unlockablesList.unlockables[29].prefabObject.transform.Find("SofaChairMesh/Cube (3)").gameObject.layer = roomLayer;
        // Fridge
        StartOfRoundRefs.Instance.unlockablesList.unlockables[30].prefabObject.transform.Find("FridgeBody").gameObject.layer = roomLayer;
        // Electric Chair
        StartOfRoundRefs.Instance.unlockablesList.unlockables[32].prefabObject.transform.Find("ElectricChair").gameObject.layer = roomLayer;
        // Dog House
        StartOfRoundRefs.Instance.unlockablesList.unlockables[33].prefabObject.transform.Find("DoghouseMesh").gameObject.layer = roomLayer;
    }

    private static void RegisterDawnUnlockables(On.Terminal.orig_Awake orig, Terminal self)
    {
        if (LethalContent.Unlockables.IsFrozen)
        {
            orig(self);
            return;
        }

        int latestUnlockableID = StartOfRoundRefs.Instance.unlockablesList.unlockables.Count;
        Debuggers.Unlockables?.Log($"latestUnlockableID = {latestUnlockableID}");

        Terminal terminal = TerminalRefs.Instance;
        TerminalKeyword confirmPurchaseKeyword = TerminalRefs.ConfirmPurchaseKeyword;
        TerminalKeyword denyPurchaseKeyword = TerminalRefs.DenyKeyword;
        TerminalNode cancelPurchaseNode = TerminalRefs.CancelPurchaseNode;
        TerminalKeyword buyKeyword = TerminalRefs.BuyKeyword;
        TerminalKeyword infoKeyword = TerminalRefs.InfoKeyword;

        List<CompatibleNoun> newBuyCompatibleNouns = buyKeyword.compatibleNouns.ToList();
        List<CompatibleNoun> newInfoCompatibleNouns = infoKeyword.compatibleNouns.ToList();
        List<TerminalKeyword> newTerminalKeywords = terminal.terminalNodes.allKeywords.ToList();
        foreach (DawnUnlockableItemInfo unlockableInfo in LethalContent.Unlockables.Values)
        {
            if (unlockableInfo.ShouldSkipIgnoreOverride())
                continue;

            StartOfRoundRefs.Instance.unlockablesList.unlockables.Add(unlockableInfo.UnlockableItem);
            if (unlockableInfo.UnlockableItem.alreadyUnlocked || unlockableInfo.RequestNode == null)
            {
                PlaceableShipObject? alreadyUnlockedPlaceableShipObject = unlockableInfo.UnlockableItem.prefabObject?.GetComponentInChildren<PlaceableShipObject>();
                if (alreadyUnlockedPlaceableShipObject == null)
                    continue;

                alreadyUnlockedPlaceableShipObject.parentObject.unlockableID = StartOfRoundRefs.Instance.unlockablesList.unlockables.Count;
                alreadyUnlockedPlaceableShipObject.unlockableID = StartOfRoundRefs.Instance.unlockablesList.unlockables.Count;
                continue;
            }

            unlockableInfo.RequestNode.shipUnlockableID = latestUnlockableID;
            latestUnlockableID++;

            UpdateUnlockablePrices(unlockableInfo);

            unlockableInfo.RequestNode.terminalOptions[0].noun = confirmPurchaseKeyword;
            unlockableInfo.RequestNode.terminalOptions[0].result = CreateUnlockableConfirmNode(unlockableInfo.UnlockableItem, unlockableInfo.RequestNode.shipUnlockableID);

            unlockableInfo.ConfirmNode = unlockableInfo.RequestNode.terminalOptions[0].result;

            unlockableInfo.RequestNode.terminalOptions[1].noun = denyPurchaseKeyword;
            unlockableInfo.RequestNode.terminalOptions[1].result = cancelPurchaseNode;

            unlockableInfo.BuyKeyword = new TerminalKeywordBuilder($"Buy{unlockableInfo.UnlockableItem.unlockableName}", $"{unlockableInfo.UnlockableItem.unlockableName.ToLowerInvariant()}", ITerminalKeyword.DawnKeywordType.Store)
                .SetDefaultVerb(buyKeyword)
                .Build();

            newTerminalKeywords.Add(unlockableInfo.BuyKeyword);

            if (unlockableInfo.InfoNode != null)
            {
                newInfoCompatibleNouns.Add(new CompatibleNoun()
                {
                    noun = unlockableInfo.BuyKeyword,
                    result = unlockableInfo.InfoNode
                });
            }

            newBuyCompatibleNouns.Add(new CompatibleNoun()
            {
                noun = unlockableInfo.BuyKeyword,
                result = unlockableInfo.RequestNode
            });


            PlaceableShipObject? placeableShipObject = unlockableInfo.UnlockableItem.prefabObject?.GetComponentInChildren<PlaceableShipObject>();
            if (placeableShipObject != null)
            {
                placeableShipObject.parentObject.unlockableID = unlockableInfo.RequestNode.shipUnlockableID;
                placeableShipObject.unlockableID = unlockableInfo.RequestNode.shipUnlockableID;
            }
        }

        buyKeyword.compatibleNouns = newBuyCompatibleNouns.ToArray();
        infoKeyword.compatibleNouns = newInfoCompatibleNouns.ToArray();
        terminal.terminalNodes.allKeywords = newTerminalKeywords.ToArray();
        orig(self);
    }

    private static readonly Regex UpgradeLineRegex = new(@"(?m)^\* (?<name>.+?)(?<tail>\s+//\s+Price:\s+\$\d+.*)$", RegexOptions.Compiled);

    private static readonly Regex AnyUpgradeLineRegex = new(@"(?m)^\* (?<name>.+?)\s+//\s+Price:\s+\$\d+.*$", RegexOptions.Compiled);

    private static string AddShipUpgradesToTerminal(On.Terminal.orig_TextPostProcess orig, Terminal self, string modifiedDisplayText, TerminalNode node)
    {
        string text = orig(self, modifiedDisplayText, node);

        int headerIdx = text.IndexOf("Ship upgrades", StringComparison.OrdinalIgnoreCase);
        if (headerIdx < 0)
            return text;

        int blockStart = text.IndexOf('\n', headerIdx);
        if (blockStart < 0)
            return text;

        int blockEnd = text.IndexOf("\n\n>", blockStart, StringComparison.Ordinal);
        if (blockEnd < 0)
        {
            blockEnd = text.Length;
        }

        string before = text[..blockStart];
        string block = text[blockStart..blockEnd];
        string after = text[blockEnd..];

        Dictionary<string, string> overrides = new(StringComparer.OrdinalIgnoreCase);

        foreach (DawnUnlockableItemInfo unlockableInfo in LethalContent.Unlockables.Values)
        {
            UpdateUnlockablePrices(unlockableInfo);

            string vanillaName = unlockableInfo.UnlockableItem.unlockableName ?? string.Empty;
            if (string.IsNullOrWhiteSpace(vanillaName))
                continue;

            TerminalPurchaseResult result = unlockableInfo.DawnPurchaseInfo.PurchasePredicate.CanPurchase();
            if (result is TerminalPurchaseResult.FailedPurchaseResult failed && !string.IsNullOrWhiteSpace(failed.OverrideName))
            {
                overrides[vanillaName] = failed.OverrideName!;
            }
        }

        HashSet<string> existingNames = new(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in AnyUpgradeLineRegex.Matches(block))
        {
            string name = match.Groups["name"].Value.Trim();
            if (!string.IsNullOrWhiteSpace(name))
            {
                existingNames.Add(name);
            }
        }

        List<string> linesToAdd = new();

        foreach (DawnUnlockableItemInfo unlockableInfo in LethalContent.Unlockables.Values)
        {
            if (!unlockableInfo.UnlockableItem.alwaysInStock)
                continue;

            string vanillaName = unlockableInfo.UnlockableItem.unlockableName ?? string.Empty;
            if (string.IsNullOrWhiteSpace(vanillaName))
                continue;

            string displayName = overrides.TryGetValue(vanillaName, out string overridden) ? overridden : vanillaName;

            if (existingNames.Contains(vanillaName) || existingNames.Contains(displayName))
                continue;

            int cost = unlockableInfo.RequestNode?.itemCost ?? 0;
            linesToAdd.Add($"* {displayName}    //    Price: ${cost}");
            existingNames.Add(displayName);
        }

        if (overrides.Count > 0)
        {
            block = UpgradeLineRegex.Replace(block, m =>
            {
                string currentName = m.Groups["name"].Value.Trim();
                if (overrides.TryGetValue(currentName, out string newName))
                {
                    return $"* {newName}{m.Groups["tail"].Value}";
                }

                return m.Value;
            });
        }

        if (linesToAdd.Count > 0)
        {
            string insertion = "\n" + string.Join("\n", linesToAdd);

            MatchCollection matches = AnyUpgradeLineRegex.Matches(block);
            if (matches.Count > 0)
            {
                Match last = matches[^1];
                int insertPos = last.Index + last.Length;
                block = block.Insert(insertPos, insertion);
            }
            else
            {
                block += insertion;
            }
        }

        return before + block + after;
    }

    internal static void UpdateAllUnlockablePrices()
    {
        foreach (DawnUnlockableItemInfo info in LethalContent.Unlockables.Values)
        {
            if (info.ShouldSkipRespectOverride())
                continue;

            UpdateUnlockablePrices(info);
        }
    }

    static void UpdateUnlockablePrices(DawnUnlockableItemInfo info)
    {
        int cost = info.DawnPurchaseInfo.Cost.Provide();
        if (info.RequestNode != null)
        {
            info.RequestNode.itemCost = cost;
        }

        if (info.ConfirmNode != null)
        {
            info.ConfirmNode.itemCost = cost;
        }
    }


    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Start)), HarmonyPrefix, HarmonyAfter("x753.More_Suits")]
    private static void FreezeShipUnlockables()
    {
        if (LethalContent.Unlockables.IsFrozen)
        {
            return;
        }

        foreach (UnlockableItem unlockableItem in StartOfRoundRefs.Instance.unlockablesList.unlockables)
        {
            if (unlockableItem.HasDawnInfo())
                continue;

            string name = NamespacedKey.NormalizeStringForNamespacedKey(unlockableItem.unlockableName, true);
            NamespacedKey<DawnUnlockableItemInfo>? key = UnlockableItemKeys.GetByReflection(name);
            if (key == null && LethalLibCompat.Enabled && LethalLibCompat.TryGetUnlockableItemFromLethalLib(unlockableItem, out string lethalLibModName))
            {
                key = NamespacedKey<DawnUnlockableItemInfo>.From(NamespacedKey.NormalizeStringForNamespacedKey(lethalLibModName, false), NamespacedKey.NormalizeStringForNamespacedKey(unlockableItem.unlockableName, false));
            }
            else if (key == null)
            {
                key = NamespacedKey<DawnUnlockableItemInfo>.From("unknown_lib", NamespacedKey.NormalizeStringForNamespacedKey(unlockableItem.unlockableName, false));
            }

            if (LethalContent.Unlockables.ContainsKey(key))
            {
                DawnPlugin.Logger.LogWarning($"UnlockableItem {unlockableItem.unlockableName} is already registered by the same creator to LethalContent. This is likely to cause issues.");
                unlockableItem.SetDawnInfo(LethalContent.Unlockables[key]);
                continue;
            }

            int cost = 0;
            if (unlockableItem.shopSelectionNode == null && !unlockableItem.alreadyUnlocked && !unlockableItem.alwaysInStock)
            {
                // this is probably a problem?
                DawnPlugin.Logger.LogWarning($"Unlockable {unlockableItem.unlockableName} has no shop selection node and is not already unlocked. This is probably a problem.");
            }
            else if (unlockableItem.shopSelectionNode != null)
            {
                cost = unlockableItem.shopSelectionNode.itemCost;
            }

            DawnSuitInfo? suitInfo = null;
            if (unlockableItem.suitMaterial != null)
            {
                suitInfo = new DawnSuitInfo(unlockableItem.suitMaterial, unlockableItem.jumpAudio);
            }
            DawnPlaceableObjectInfo? placeableObjectInfo = null;
            if (unlockableItem.prefabObject != null)
            {
                placeableObjectInfo = new DawnPlaceableObjectInfo();
            }

            TerminalNode? requestNode = unlockableItem.shopSelectionNode;
            TerminalNode? confirmNode = unlockableItem.shopSelectionNode?.terminalOptions?.FirstOrDefault()?.result;
            TerminalKeyword? unlockableBuyKeyword = null;
            if (requestNode != null)
            {
                unlockableBuyKeyword = TerminalRefs.BuyKeyword.compatibleNouns.Where(x => x.result == requestNode).Select(x => x.noun).FirstOrDefault();
            }
            TerminalNode? infoNode = null;
            if (requestNode != null)
            {
                infoNode = TerminalRefs.InfoKeyword.compatibleNouns.Where(x => x.noun == unlockableBuyKeyword)?.Select(x => x.result).FirstOrDefault();
            }

            DawnUnlockableItemInfo unlockableItemInfo = new(key, [DawnLibTags.IsExternal], unlockableItem, new DawnPurchaseInfo(new SimpleProvider<int>(cost), ITerminalPurchasePredicate.AlwaysSuccess()), suitInfo, placeableObjectInfo, requestNode, confirmNode, unlockableBuyKeyword, infoNode, null);
            unlockableItem.SetDawnInfo(unlockableItemInfo);
            LethalContent.Unlockables.Register(unlockableItemInfo);
        }

        LethalContent.Unlockables.Freeze();
    }

    private static TerminalNode CreateUnlockableConfirmNode(UnlockableItem unlockableItem, int shipUnlockableID)
    {
        TerminalNode terminalNode = new TerminalNodeBuilder($"{unlockableItem.unlockableName}ConfirmNode")
            .SetDisplayText($"Ordered the {unlockableItem.unlockableName}! Your new balance is [playerCredits].\nPress [B] to rearrange objects in your ship and [V] to confirm.")
            .SetClearPreviousText(true)
            .SetMaxCharactersToType(35)
            .SetShipUnlockableIndex(shipUnlockableID)
            .SetBuyUnlockable(true)
            .SetPlaySyncedClip(0)
            .Build();

        return terminalNode;
    }
}