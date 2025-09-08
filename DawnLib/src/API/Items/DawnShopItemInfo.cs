using Dawn.Internal;

namespace Dawn;
public sealed class DawnShopItemInfo : ITerminalPurchase
{
    public DawnItemInfo ParentInfo { get; internal set; }

    internal DawnShopItemInfo(ITerminalPurchasePredicate predicate, TerminalNode? infoNode, TerminalNode requestNode, TerminalNode receiptNode, IProvider<int> cost)
    {
        PurchasePredicate = predicate;
        InfoNode = infoNode;
        RequestNode = requestNode;
        ReceiptNode = receiptNode;
        Cost = cost;
    }

    public TerminalNode? InfoNode { get; }
    public TerminalNode RequestNode { get; }
    public TerminalNode ReceiptNode { get; }
    public IProvider<int> Cost { get; }
    public ITerminalPurchasePredicate PurchasePredicate { get; }
    
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
        Terminal terminal = TerminalRefs.Instance;
        return terminal.itemSalesPercentages[RequestNode.buyItemIndex];
    }
}