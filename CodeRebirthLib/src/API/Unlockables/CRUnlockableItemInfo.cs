namespace CodeRebirthLib;

public sealed class CRUnlockableItemInfo : CRBaseInfo<CRUnlockableItemInfo>, ITerminalPurchase
{
    internal CRUnlockableItemInfo(ITerminalPurchasePredicate predicate, NamespacedKey<CRUnlockableItemInfo> key, bool isExternal, UnlockableItem unlockableItem, int cost, CRSuitInfo? suitInfo, CRPlaceableObjectInfo? placeableObjectInfo) : base(key, isExternal)
    {
        PurchasePredicate = predicate;
        UnlockableItem = unlockableItem;
        SuitInfo = suitInfo;
        if (SuitInfo != null) SuitInfo.ParentInfo = this;
        PlaceableObjectInfo = placeableObjectInfo;
        if (PlaceableObjectInfo != null) PlaceableObjectInfo.ParentInfo = this;
        Cost = cost;
    }

    public UnlockableItem UnlockableItem { get; }
    public int Cost { get; }
    public ITerminalPurchasePredicate PurchasePredicate { get; }
    public CRSuitInfo? SuitInfo { get; }
    public CRPlaceableObjectInfo? PlaceableObjectInfo { get; }
}