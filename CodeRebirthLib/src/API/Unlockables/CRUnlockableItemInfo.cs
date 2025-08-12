namespace CodeRebirthLib;

public sealed class CRUnlockableItemInfo : INamespaced<CRUnlockableItemInfo>, ITerminalPurchase
{
    internal CRUnlockableItemInfo(ITerminalPurchasePredicate predicate, NamespacedKey<CRUnlockableItemInfo> key, UnlockableItem unlockableItem, int cost, CRSuitInfo? suitInfo, CRPlaceableObjectInfo? placeableObjectInfo)
    {
        PurchasePredicate = predicate;
        UnlockableItem = unlockableItem;
        SuitInfo = suitInfo;
        if (SuitInfo != null) SuitInfo.ParentInfo = this;
        PlaceableObjectInfo = placeableObjectInfo;
        if (PlaceableObjectInfo != null) PlaceableObjectInfo.ParentInfo = this;
        Cost = cost;
        TypedKey = key;
    }

    public UnlockableItem UnlockableItem { get; }
    public int Cost { get; }
    public ITerminalPurchasePredicate PurchasePredicate { get; }
    public CRSuitInfo? SuitInfo { get; }
    public CRPlaceableObjectInfo? PlaceableObjectInfo { get; }
    public NamespacedKey Key => TypedKey;
    public NamespacedKey<CRUnlockableItemInfo> TypedKey { get; }
}