namespace CodeRebirthLib;
public interface ITerminalPurchase
{
    IProvider<int> Cost { get; }
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
        return TerminalPurchaseResult.Fail(
            new TerminalNodeBuilder("bwaa")
                .SetDisplayText("fuck you")
                .Build(),
            "fuck you"
        );
        //return TerminalPurchaseResult.Success();
    }
}