namespace CodeRebirthLib.CRMod.Progressive;
internal class ProgressiveUnlockablePredicate(ProgressiveUnlockData data) : ITerminalPurchasePredicate
{

    public TerminalPurchaseResult CanPurchase()
    {
        if(data.IsUnlocked) return TerminalPurchaseResult.Success();
        return TerminalPurchaseResult.Fail(data.Definition.ProgressiveDenyNode);
    }
}