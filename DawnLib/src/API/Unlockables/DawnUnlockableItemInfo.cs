using System.Collections.Generic;

namespace Dawn;

public sealed class DawnUnlockableItemInfo : DawnBaseInfo<DawnUnlockableItemInfo>
{
    internal DawnUnlockableItemInfo(NamespacedKey<DawnUnlockableItemInfo> key, HashSet<NamespacedKey> tags, UnlockableItem unlockableItem, DawnPurchaseInfo dawnPurchaseInfo, DawnSuitInfo? suitInfo, DawnPlaceableObjectInfo? placeableObjectInfo, TerminalNode? requestNode, TerminalNode? confirmNode, TerminalKeyword? buyKeyword, TerminalNode? infoNode, IDataContainer? customData) : base(key, tags, customData)
    {
        UnlockableItem = unlockableItem;
        SuitInfo = suitInfo;
        if (SuitInfo != null) SuitInfo.ParentInfo = this;
        PlaceableObjectInfo = placeableObjectInfo;
        if (PlaceableObjectInfo != null) PlaceableObjectInfo.ParentInfo = this;
        DawnPurchaseInfo = dawnPurchaseInfo;

        RequestNode = requestNode;
        ConfirmNode = confirmNode;
        BuyKeyword = buyKeyword;
        InfoNode = infoNode;
    }

    public UnlockableItem UnlockableItem { get; }
    public DawnPurchaseInfo DawnPurchaseInfo { get; }
    public DawnSuitInfo? SuitInfo { get; }
    public DawnPlaceableObjectInfo? PlaceableObjectInfo { get; }

    public int IndexInList { get; internal set; }
    public TerminalNode? RequestNode { get; internal set; }
    public TerminalNode? ConfirmNode { get; internal set; }
    public TerminalKeyword? BuyKeyword { get; internal set; }
    public TerminalNode? InfoNode { get; internal set; }
}