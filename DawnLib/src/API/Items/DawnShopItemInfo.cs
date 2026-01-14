using Dawn.Internal;

namespace Dawn;
public sealed class DawnShopItemInfo
{
    public DawnItemInfo ParentInfo { get; internal set; }

    internal DawnShopItemInfo(DawnPurchaseInfo dawnPurchaseInfo, TerminalNode? infoNode, TerminalNode requestNode, TerminalNode receiptNode)
    {
        DawnPurchaseInfo = dawnPurchaseInfo;
        InfoNode = infoNode;
        RequestNode = requestNode;
        ReceiptNode = receiptNode;
    }

    public TerminalNode? InfoNode { get; private set; }
    public TerminalNode RequestNode { get; }
    public TerminalNode ReceiptNode { get; }
    public DawnPurchaseInfo DawnPurchaseInfo { get; }

    public void AddToDropship(bool ignoreMax = false, int count = 1)
    {
        Terminal terminal = TerminalRefs.Instance;
        for (int i = 0; i < count; i++)
        {
            if (!ignoreMax && terminal.orderedItemsFromTerminal.Count > 12)
            {
                break;
            }
            terminal.orderedItemsFromTerminal.Add(RequestNode.buyItemIndex);
            terminal.numberOfItemsInDropship++;
        }
    }

    public int GetSalePercentage()
    {
        return TerminalRefs.Instance.itemSalesPercentages[RequestNode.buyItemIndex];
    }
}