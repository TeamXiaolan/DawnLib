using System.Linq;
using CodeRebirthLib.CRMod.Progressive;
using UnityEngine;

namespace CodeRebirthLib;

static class UnlockableRegistrationHandler
{
    internal static void Init()
    {
        On.Terminal.LoadNewNodeIfAffordable += Terminal_LoadNewNodeIfAffordable;
        On.Terminal.Awake += RegisterShipUnlockables; // maybe patch terminal instead and await for startofround's instance to be not null?
    }

    private static void RegisterShipUnlockables(On.Terminal.orig_Awake orig, Terminal self)
    {
        if (LethalContent.Unlockables.IsFrozen)
        {
            orig(self);
            return;
        }

        TerminalKeyword buyKeyword = self.terminalNodes.allKeywords.First(keyword => keyword.word == "buy");
        TerminalKeyword confirmPurchaseKeyword = self.terminalNodes.allKeywords.First(keyword2 => keyword2.word == "confirm");
        TerminalKeyword denyPurchaseKeyword = self.terminalNodes.allKeywords.First(keyword2 => keyword2.word == "deny");
        TerminalNode cancelPurchaseNode = buyKeyword.compatibleNouns[0].result.terminalOptions[1].result; // TODO, I use these a couple times, maybe i should have em stored somewhere in LethalContent?

        UnlockableItem latestValidUnlockable = StartOfRound.Instance.unlockablesList.unlockables.Where(unlockable => unlockable.shopSelectionNode != null).OrderBy(x => x.shopSelectionNode.shipUnlockableID).FirstOrDefault();
        int latestUnlockableID = latestValidUnlockable.shopSelectionNode.shipUnlockableID;
        Debuggers.ReplaceThis?.Log($"latestUnlockableID = {latestUnlockableID}");

        foreach (CRUnlockableItemInfo unlockableInfo in LethalContent.Unlockables.Values)
        {
            CRPlaceableObjectInfo? placeableObjectInfo = unlockableInfo.PlaceableObjectInfo;
            if (placeableObjectInfo == null || unlockableInfo.Key.IsVanilla())
                continue; // also ensure not to register vanilla stuff again

            StartOfRound.Instance.unlockablesList.unlockables.Add(unlockableInfo.UnlockableItem);
            TerminalNode shopSelectionNode = ScriptableObject.CreateInstance<TerminalNode>(); // unsure if its relevant but for some reason some ship upgrades dont have this, might not be needed and game might auto generate them.
            shopSelectionNode.displayText = $"You have requested to order a {unlockableInfo.UnlockableItem.unlockableName.ToLowerInvariant()}.\nTotal cost of item: [totalCost].\n\nPlease CONFIRM or DENY.";
            shopSelectionNode.clearPreviousText = true;
            shopSelectionNode.maxCharactersToType = 25;
            shopSelectionNode.shipUnlockableID = latestUnlockableID;
            shopSelectionNode.itemCost = unlockableInfo.Cost;
            shopSelectionNode.creatureName = unlockableInfo.UnlockableItem.unlockableName;
            shopSelectionNode.overrideOptions = true;

            CompatibleNoun confirmBuyCompatibleNoun = new();
            confirmBuyCompatibleNoun.noun = confirmPurchaseKeyword;
            confirmBuyCompatibleNoun.result = CreateUnlockableConfirmNode(unlockableInfo.UnlockableItem, latestUnlockableID, unlockableInfo.Cost);

            CompatibleNoun cancelDenyCompatibleNoun = new();
            cancelDenyCompatibleNoun.noun = denyPurchaseKeyword;
            cancelDenyCompatibleNoun.result = cancelPurchaseNode;

            shopSelectionNode.terminalOptions = [confirmBuyCompatibleNoun, cancelDenyCompatibleNoun];
            unlockableInfo.UnlockableItem.shopSelectionNode = shopSelectionNode;
            latestUnlockableID++;
            // TODO: bruh while doing this i just realised decors are literally just ship upgrades without "Always In Stock" ticked, like i thought it'd be more than that lol but fair enough ig
        }

        foreach (CRUnlockableItemInfo unlockableInfo in LethalContent.Unlockables.Values)
        {
            CRSuitInfo? suitInfo = unlockableInfo.SuitInfo;
            if (suitInfo == null || unlockableInfo.Key.IsVanilla())
                continue; // also ensure not to register vanilla stuff again

            // TODO Suits
        }

        LethalContent.Unlockables.Freeze();
        orig(self);
    }

    private static TerminalNode CreateUnlockableConfirmNode(UnlockableItem unlockableItem, int latestUnlockableID, int cost)
    {
        TerminalNode terminalNode = ScriptableObject.CreateInstance<TerminalNode>();
        terminalNode.displayText = $"Ordered the {unlockableItem.unlockableName.ToLowerInvariant()}! Your new balance is [playerCredits].\nPress [B] to rearrange objects in your ship and [V] to confirm.";
        terminalNode.clearPreviousText = true;
        terminalNode.maxCharactersToType = 35;
        terminalNode.shipUnlockableID = latestUnlockableID;
        terminalNode.buyUnlockable = true;
        terminalNode.itemCost = cost;
        terminalNode.playSyncedClip = 0;
        return terminalNode;
    }

    private static void Terminal_LoadNewNodeIfAffordable(On.Terminal.orig_LoadNewNodeIfAffordable orig, Terminal self, TerminalNode node)
    {
        if (node.shipUnlockableID != -1)
        {
            UnlockableItem unlockableItem = StartOfRound.Instance.unlockablesList.unlockables[node.shipUnlockableID];
            ProgressiveUnlockData? unlockData = ProgressiveUnlockableHandler.AllProgressiveUnlockables
                .FirstOrDefault(it => it.Definition.UnlockableItem == unlockableItem);

            if (unlockData != null && !unlockData.IsUnlocked)
            {
                orig(self, unlockData.Definition.ProgressiveDenyNode);
                return;
            }
        }
        orig(self, node);
    }
}