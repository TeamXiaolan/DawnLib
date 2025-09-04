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
    
    // todo: these are currently nullable because they aren't collected from vanilla/modded sources
    public TerminalNode? RequestNode { get; internal set; }
    public TerminalNode? ConfirmNode { get; internal set; }
}