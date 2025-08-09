namespace CodeRebirthLib;
public sealed class CRShopItemInfo
{
    public CRItemInfo ParentInfo { get; internal set; }
    
    internal CRShopItemInfo(TerminalNode infoNode, TerminalNode requestNode, TerminalNode receiptNode, int cost)
    {
        InfoNode = infoNode;
        RequestNode = requestNode;
        ReceiptNode = receiptNode;
        Cost = cost;
    }
    
    public TerminalNode InfoNode { get; }
    public TerminalNode RequestNode { get; }
    public TerminalNode ReceiptNode { get; }
    public int Cost { get; }
}