namespace Dawn;
public interface ITerminalPurchase
{
    IProvider<int> Cost { get; }
    ITerminalPurchasePredicate PurchasePredicate { get; }

}

public interface ITerminalPurchasePredicate
{
    TerminalPurchaseResult CanPurchase();

    public static ITerminalPurchasePredicate AlwaysSuccess() { return new ConstantTerminalPredicate(TerminalPurchaseResult.Success()); }
    public static ITerminalPurchasePredicate AlwaysHide() { return new ConstantTerminalPredicate(TerminalPurchaseResult.Hidden()); }
    public static ITerminalPurchasePredicate AlwaysFail(TerminalNode fail) { return new ConstantTerminalPredicate(TerminalPurchaseResult.Fail(fail)); }
}

internal class ConstantTerminalPredicate(TerminalPurchaseResult result) : ITerminalPurchasePredicate
{
    public TerminalPurchaseResult CanPurchase()
    {
        return result;
    }
}