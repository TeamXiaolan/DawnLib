using System.Linq;
using CodeRebirthLib.CRMod;
using CodeRebirthLib.Internal;
using UnityEngine;

namespace CodeRebirthLib;

static class UnlockableRegistrationHandler
{
    internal static void Init()
    {
        On.Terminal.Awake += RegisterShipUnlockables;
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
        Debuggers.Unlockables?.Log($"latestUnlockableID = {latestUnlockableID}");

        foreach (CRUnlockableItemInfo unlockableInfo in LethalContent.Unlockables.Values)
        {
            CRPlaceableObjectInfo? placeableObjectInfo = unlockableInfo.PlaceableObjectInfo;
            if (placeableObjectInfo == null || unlockableInfo.HasTag(CRLibTags.IsExternal))
                continue;

            StartOfRound.Instance.unlockablesList.unlockables.Add(unlockableInfo.UnlockableItem);
            TerminalNode shopSelectionNode = ScriptableObject.CreateInstance<TerminalNode>(); // unsure if its relevant but for some reason some ship upgrades dont have this, might not be needed and game might auto generate them.
            shopSelectionNode.displayText = $"You have requested to order a {unlockableInfo.UnlockableItem.unlockableName.ToLowerInvariant()}.\nTotal cost of item: [totalCost].\n\nPlease CONFIRM or DENY.";
            shopSelectionNode.clearPreviousText = true;
            shopSelectionNode.maxCharactersToType = 25;
            shopSelectionNode.shipUnlockableID = latestUnlockableID;
            shopSelectionNode.itemCost = unlockableInfo.Cost.Provide();
            shopSelectionNode.creatureName = unlockableInfo.UnlockableItem.unlockableName;
            shopSelectionNode.overrideOptions = true;

            CompatibleNoun confirmBuyCompatibleNoun = new();
            confirmBuyCompatibleNoun.noun = confirmPurchaseKeyword;
            confirmBuyCompatibleNoun.result = CreateUnlockableConfirmNode(unlockableInfo.UnlockableItem, latestUnlockableID, unlockableInfo.Cost.Provide());

            CompatibleNoun cancelDenyCompatibleNoun = new();
            cancelDenyCompatibleNoun.noun = denyPurchaseKeyword;
            cancelDenyCompatibleNoun.result = cancelPurchaseNode;

            shopSelectionNode.terminalOptions = [confirmBuyCompatibleNoun, cancelDenyCompatibleNoun];
            unlockableInfo.UnlockableItem.shopSelectionNode = shopSelectionNode;
            latestUnlockableID++;
        }

        foreach (CRUnlockableItemInfo unlockableInfo in LethalContent.Unlockables.Values)
        {
            CRSuitInfo? suitInfo = unlockableInfo.SuitInfo;
            if (suitInfo == null || unlockableInfo.HasTag(CRLibTags.IsExternal))
                continue; // also ensure not to register vanilla stuff again

            // TODO Suits
        }

        foreach (UnlockableItem unlockableItem in StartOfRound.Instance.unlockablesList.unlockables)
        {
            if (unlockableItem.TryGetCRInfo(out _))
                continue;

            string name = NamespacedKey.NormalizeStringForNamespacedKey(unlockableItem.unlockableName, true);
            NamespacedKey<CRUnlockableItemInfo>? key = UnlockableItemKeys.GetByReflection(name);
            key ??= NamespacedKey<CRUnlockableItemInfo>.From("modded_please_replace_this_later", NamespacedKey.NormalizeStringForNamespacedKey(unlockableItem.unlockableName, false));
            int cost = 0;
            if (unlockableItem.shopSelectionNode == null && unlockableItem.alreadyUnlocked)
            {
                // this is probably a problem?
                Debuggers.Unlockables?.Log($"Unlockable {unlockableItem.unlockableName} has no shop selection node and is already unlocked. This is probably a problem.");
            }
            else if (unlockableItem.shopSelectionNode != null)
            {
                cost = unlockableItem.shopSelectionNode.itemCost;
            }

            CRSuitInfo? suitInfo = null;
            if (unlockableItem.suitMaterial != null)
            {
                suitInfo = new CRSuitInfo();
            }
            CRPlaceableObjectInfo? placeableObjectInfo = null;
            if (unlockableItem.prefabObject != null)
            {
                placeableObjectInfo = new CRPlaceableObjectInfo();
            }

            CRUnlockableItemInfo unlockableItemInfo = new(new AlwaysAvaliableTerminalPredicate(), key, [CRLibTags.IsExternal], unlockableItem, new SimpleProvider<int>(cost), suitInfo, placeableObjectInfo);
            LethalContent.Unlockables.Register(unlockableItemInfo);
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
}