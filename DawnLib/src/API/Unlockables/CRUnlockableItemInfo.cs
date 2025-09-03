using System.Collections.Generic;

namespace Dawn;

public sealed class CRUnlockableItemInfo : CRBaseInfo<CRUnlockableItemInfo>, ITerminalPurchase
{
    internal CRUnlockableItemInfo(ITerminalPurchasePredicate predicate, NamespacedKey<CRUnlockableItemInfo> key, List<NamespacedKey> tags, UnlockableItem unlockableItem, IProvider<int> cost, CRSuitInfo? suitInfo, CRPlaceableObjectInfo? placeableObjectInfo) : base(key, tags)
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
    public IProvider<int> Cost { get; }
    public ITerminalPurchasePredicate PurchasePredicate { get; }
    public CRSuitInfo? SuitInfo { get; }
    public CRPlaceableObjectInfo? PlaceableObjectInfo { get; }
}