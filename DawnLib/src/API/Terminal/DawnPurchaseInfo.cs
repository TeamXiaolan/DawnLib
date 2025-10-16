namespace Dawn;
public sealed class DawnPurchaseInfo : ITerminalPurchase
{
    internal DawnPurchaseInfo(IProvider<int> cost, ITerminalPurchasePredicate purchasePredicate)
    {
        Cost = cost;
        PurchasePredicate = purchasePredicate;
    }

    public IProvider<int> Cost { get; private set; }
    public ITerminalPurchasePredicate PurchasePredicate { get; private set; }
}