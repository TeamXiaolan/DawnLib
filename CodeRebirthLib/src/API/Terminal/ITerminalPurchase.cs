namespace CodeRebirthLib;
public interface ITerminalPurchase
{
    int Cost { get; }
    ITerminalPurchasePredicate PurchasePredicate { get; }

}

public interface ITerminalPurchasePredicate
{
    TerminalPurchaseResult CanPurchase();
}

public class AlwaysAvaliableTerminalPredicate : ITerminalPurchasePredicate
{
    public TerminalPurchaseResult CanPurchase()
    {
        return TerminalPurchaseResult.Success();
    }
}