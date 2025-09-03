using System.Collections.Generic;

namespace Dawn;

public sealed class DawnUnlockableItemInfo : DawnBaseInfo<DawnUnlockableItemInfo>, ITerminalPurchase
{
    internal DawnUnlockableItemInfo(ITerminalPurchasePredicate predicate, NamespacedKey<DawnUnlockableItemInfo> key, List<NamespacedKey> tags, UnlockableItem unlockableItem, IProvider<int> cost, DawnSuitInfo? suitInfo, DawnPlaceableObjectInfo? placeableObjectInfo) : base(key, tags)
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
    public DawnSuitInfo? SuitInfo { get; }
    public DawnPlaceableObjectInfo? PlaceableObjectInfo { get; }
}