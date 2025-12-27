using System.Linq;

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

internal class LethalLevelLoaderTerminalPredicate(object extendedLevelObject) : ITerminalPurchasePredicate
{
    private TerminalNode? _failNode = null;
    public TerminalPurchaseResult CanPurchase()
    {
        if (extendedLevelObject is SelectableLevel selectableLevel)
        {
            extendedLevelObject = LethalLevelLoader.LevelManager.GetExtendedLevel(selectableLevel);
        }
        LethalLevelLoader.ExtendedLevel extendedLevel = (LethalLevelLoader.ExtendedLevel)extendedLevelObject;
        if (extendedLevel.IsRouteLocked && extendedLevel.IsRouteHidden)
        {
            return TerminalPurchaseResult.Hidden().SetFailure(true);
        }
        else if (extendedLevel.IsRouteHidden)
        {
            return TerminalPurchaseResult.Hidden().SetFailure(false);
        }
        else if (extendedLevel.IsRouteLocked)
        {
            if (_failNode == null)
            {
                _failNode = new TerminalNodeBuilder($"{extendedLevel.name.Replace(" ", "").SkipWhile(x => !char.IsLetter(x)).ToArray()}LethalLevelLoaderTerminalPredicateFail")
                    .SetDisplayText($"{extendedLevel.LockedRouteNodeText}")
                    .Build();
            }
            return TerminalPurchaseResult.Fail(_failNode);
        }
        return TerminalPurchaseResult.Success();
    }
}