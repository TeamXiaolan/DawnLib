using System.Linq;
using Dawn.Internal;

namespace Dawn;

static class UnlockableRegistrationHandler
{
    internal static void Init()
    {
        On.Terminal.Awake += RegisterShipUnlockables;
    }

    internal static void UpdateAllUnlockablePrices()
    {
        foreach (DawnUnlockableItemInfo info in LethalContent.Unlockables.Values)
        {
            if (info.HasTag(DawnLibTags.IsExternal))
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

        TerminalKeyword confirmPurchaseKeyword = TerminalRefs.ConfirmKeyword;
        TerminalKeyword denyPurchaseKeyword = TerminalRefs.DenyKeyword;
        TerminalNode cancelPurchaseNode = TerminalRefs.CancelPurchaseNode;

        int latestUnlockableID = 0;
        foreach (UnlockableItem unlockableItem in StartOfRound.Instance.unlockablesList.unlockables)
        {
            if (unlockableItem.shopSelectionNode != null && unlockableItem.shopSelectionNode.shipUnlockableID > latestUnlockableID)
            {
                latestUnlockableID = unlockableItem.shopSelectionNode.shipUnlockableID;
            }
        }
        Debuggers.Unlockables?.Log($"latestUnlockableID = {latestUnlockableID}");

        foreach (DawnUnlockableItemInfo unlockableInfo in LethalContent.Unlockables.Values)
        {
            if (unlockableInfo.HasTag(DawnLibTags.IsExternal))
                continue;

            StartOfRound.Instance.unlockablesList.unlockables.Add(unlockableInfo.UnlockableItem);
            if (unlockableInfo.UnlockableItem.alreadyUnlocked || unlockableInfo.RequestNode == null)
                continue;

            unlockableInfo.RequestNode.shipUnlockableID = latestUnlockableID;
            latestUnlockableID++;

            UpdateUnlockablePrices(unlockableInfo);

            unlockableInfo.RequestNode.terminalOptions[0].noun = confirmPurchaseKeyword;
            unlockableInfo.RequestNode.terminalOptions[0].result = CreateUnlockableConfirmNode(unlockableInfo.UnlockableItem, latestUnlockableID);
            unlockableInfo.RequestNode.terminalOptions[1].noun = denyPurchaseKeyword;
            unlockableInfo.RequestNode.terminalOptions[1].result = cancelPurchaseNode;

            if (unlockableInfo.UnlockableItem.prefabObject.TryGetComponent(out AutoParentToShip autoParentToShip))
            {
                autoParentToShip.unlockableID = latestUnlockableID;
            }
        }

        foreach (UnlockableItem unlockableItem in StartOfRound.Instance.unlockablesList.unlockables)
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
                DawnPlugin.Logger.LogWarning($"UnlockableItem {unlockableItem.unlockableName} is already registered by the same creator to LethalContent. Skipping...");
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

            DawnUnlockableItemInfo unlockableItemInfo = new(ITerminalPurchasePredicate.AlwaysSuccess(), key, [DawnLibTags.IsExternal], unlockableItem, new SimpleProvider<int>(cost), suitInfo, placeableObjectInfo, null);
            LethalContent.Unlockables.Register(unlockableItemInfo);
        }

        LethalContent.Unlockables.Freeze();
        orig(self);
    }

    private static TerminalNode CreateUnlockableConfirmNode(UnlockableItem unlockableItem, int latestUnlockableID)
    {
        TerminalNode terminalNode = new TerminalNodeBuilder($"{unlockableItem.unlockableName}ConfirmNode")
            .SetDisplayText($"Ordered the {unlockableItem.unlockableName.ToLowerInvariant()}! Your new balance is [playerCredits].\nPress [B] to rearrange objects in your ship and [V] to confirm.")
            .SetClearPreviousText(true)
            .SetMaxCharactersToType(35)
            .SetShipUnlockableIndex(latestUnlockableID)
            .SetBuyUnlockable(true)
            .SetPlaySyncedClip(0)
            .Build();

        return terminalNode;
    }
}