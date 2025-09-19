using System.Collections.Generic;

namespace Dawn;

public sealed class DawnUnlockableItemInfo : DawnBaseInfo<DawnUnlockableItemInfo>, ITerminalPurchase
{
    internal DawnUnlockableItemInfo(ITerminalPurchasePredicate predicate, NamespacedKey<DawnUnlockableItemInfo> key, HashSet<NamespacedKey> tags, UnlockableItem unlockableItem, IProvider<int> cost, DawnSuitInfo? suitInfo, DawnPlaceableObjectInfo? placeableObjectInfo, TerminalNode? requestNode, TerminalNode? confirmNode, TerminalKeyword? buyKeyword, TerminalNode? infoNode, IDataContainer? customData) : base(key, tags, customData)
    {
        PurchasePredicate = predicate;
        UnlockableItem = unlockableItem;
        SuitInfo = suitInfo;
        if (SuitInfo != null) SuitInfo.ParentInfo = this;
        PlaceableObjectInfo = placeableObjectInfo;
        if (PlaceableObjectInfo != null) PlaceableObjectInfo.ParentInfo = this;
        Cost = cost;

        RequestNode = requestNode;
        ConfirmNode = confirmNode;
        BuyKeyword = buyKeyword;
        InfoNode = infoNode;
    }

    public UnlockableItem UnlockableItem { get; }
    public IProvider<int> Cost { get; }
    public ITerminalPurchasePredicate PurchasePredicate { get; }
    public DawnSuitInfo? SuitInfo { get; }
    public DawnPlaceableObjectInfo? PlaceableObjectInfo { get; }

    public TerminalNode? RequestNode { get; internal set; }
    public TerminalNode? ConfirmNode { get; internal set; }
    public TerminalKeyword? BuyKeyword { get; internal set; }
    public TerminalNode? InfoNode { get; internal set; }
}