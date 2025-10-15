using System.Collections.Generic;
using System.Linq;
using Dawn.Internal;
using MonoMod.RuntimeDetour;

namespace Dawn;

static class UnlockableRegistrationHandler
{
    internal static void Init()
    {
        using (new DetourContext(priority: int.MaxValue))
        {
            On.Terminal.Awake += RegisterShipUnlockables;
        }

        On.Terminal.TextPostProcess += AddShipUpgradesToTerminal;
    }

    private static string AddShipUpgradesToTerminal(On.Terminal.orig_TextPostProcess orig, Terminal self, string modifiedDisplayText, TerminalNode node)
    {
        if (modifiedDisplayText.Contains("[buyableItemsList]") && modifiedDisplayText.Contains("[unlockablesSelectionList]"))
        {
            int index = modifiedDisplayText.IndexOf(@":");

            // example: "* Loud horn    //    Price: $150"
            foreach (DawnUnlockableItemInfo unlockableItemInfo in LethalContent.Unlockables.Values)
            {
                if (unlockableItemInfo.ShouldSkipIgnoreOverride())
                    continue;

                if (!unlockableItemInfo.UnlockableItem.alwaysInStock)
                    continue; // skip decors

                string? unlockableName = unlockableItemInfo.UnlockableItem.unlockableName;
                TerminalPurchaseResult result = unlockableItemInfo.PurchasePredicate.CanPurchase();
                if (result is TerminalPurchaseResult.FailedPurchaseResult failedResult)
                {
                    if (!string.IsNullOrEmpty(failedResult.OverrideName))
                    {
                        Debuggers.Unlockables?.Log($"Overriding name of {unlockableItemInfo.Key} with {failedResult.OverrideName}");
                    }
                    unlockableName = failedResult.OverrideName;
                }

                UpdateUnlockablePrices(unlockableItemInfo);

                string newLine = $"\n* {unlockableName ?? string.Empty}    //    Price: ${unlockableItemInfo.RequestNode?.itemCost ?? 0}";

                modifiedDisplayText = modifiedDisplayText.Insert(index + 1, newLine);
            }
        }

        return orig(self, modifiedDisplayText, node);
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
        int cost = info.Cost.Provide();
        if (info.RequestNode != null)
        {
            info.RequestNode.itemCost = cost;
        }

        if (info.ConfirmNode != null)
        {
            info.ConfirmNode.itemCost = cost;
        }
    }

    private static void RegisterShipUnlockables(On.Terminal.orig_Awake orig, Terminal self)
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
                continue;

            unlockableInfo.RequestNode.shipUnlockableID = latestUnlockableID;
            latestUnlockableID++;

            UpdateUnlockablePrices(unlockableInfo);

            unlockableInfo.RequestNode.terminalOptions[0].noun = confirmPurchaseKeyword;
            unlockableInfo.RequestNode.terminalOptions[0].result = CreateUnlockableConfirmNode(unlockableInfo.UnlockableItem, unlockableInfo.RequestNode.shipUnlockableID);

            unlockableInfo.ConfirmNode = unlockableInfo.RequestNode.terminalOptions[0].result;

            unlockableInfo.RequestNode.terminalOptions[1].noun = denyPurchaseKeyword;
            unlockableInfo.RequestNode.terminalOptions[1].result = cancelPurchaseNode;

            unlockableInfo.BuyKeyword = new TerminalKeywordBuilder($"Buy{unlockableInfo.UnlockableItem.unlockableName}")
                .SetWord($"{unlockableInfo.UnlockableItem.unlockableName.ToLowerInvariant()}")
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


            PlaceableShipObject placeableShipObject = unlockableInfo.UnlockableItem.prefabObject.GetComponentInChildren<PlaceableShipObject>();
            if (placeableShipObject != null)
            {
                placeableShipObject.parentObject.unlockableID = unlockableInfo.RequestNode.shipUnlockableID;
                placeableShipObject.unlockableID = unlockableInfo.RequestNode.shipUnlockableID;
            }
        }

        buyKeyword.compatibleNouns = newBuyCompatibleNouns.ToArray();
        infoKeyword.compatibleNouns = newInfoCompatibleNouns.ToArray();
        terminal.terminalNodes.allKeywords = newTerminalKeywords.ToArray();

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
            if (unlockableItem.shopSelectionNode == null && !unlockableItem.alreadyUnlocked)
            {
                // this is probably a problem?
                Debuggers.Unlockables?.Log($"Unlockable {unlockableItem.unlockableName} has no shop selection node and is not already unlocked. This is probably a problem.");
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
                infoNode = infoKeyword.compatibleNouns.Where(x => x.noun == unlockableBuyKeyword)?.Select(x => x.result).FirstOrDefault();
            }

            DawnUnlockableItemInfo unlockableItemInfo = new(ITerminalPurchasePredicate.AlwaysSuccess(), key, [DawnLibTags.IsExternal], unlockableItem, new SimpleProvider<int>(cost), suitInfo, placeableObjectInfo, requestNode, confirmNode, unlockableBuyKeyword, infoNode, null);
            unlockableItem.SetDawnInfo(unlockableItemInfo);
            LethalContent.Unlockables.Register(unlockableItemInfo);
        }

        for (int i = 0; i < StartOfRoundRefs.Instance.unlockablesList.unlockables.Count; i++)
        {
            UnlockableItem unlockableItem = StartOfRoundRefs.Instance.unlockablesList.unlockables[i];
            if (!unlockableItem.HasDawnInfo())
                continue;

            unlockableItem.GetDawnInfo().IndexInList = i;
        }
        LethalContent.Unlockables.Freeze();
        orig(self);
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