namespace CodeRebirthLib;
public sealed class CRShopItemInfo : ITerminalPurchase
{
    public CRItemInfo ParentInfo { get; internal set; }
    
    internal CRShopItemInfo(ITerminalPurchasePredicate predicate, TerminalNode infoNode, TerminalNode requestNode, TerminalNode receiptNode, int cost)
    {
        PurchasePredicate = predicate;
        InfoNode = infoNode;
        RequestNode = requestNode;
        ReceiptNode = receiptNode;
        Cost = cost;
    }
    
    public TerminalNode InfoNode { get; }
    public TerminalNode RequestNode { get; }
    public TerminalNode ReceiptNode { get; }
    public int Cost { get; }
    public ITerminalPurchasePredicate PurchasePredicate { get; }
}