namespace CodeRebirthLib.CRMod;
internal class ProgressiveItemPredicate(ProgressiveItemData data) : ITerminalPurchasePredicate
{
    public TerminalPurchaseResult CanPurchase()
    {
        if (data.IsUnlocked)
            return TerminalPurchaseResult.Success();

        return TerminalPurchaseResult.Fail(data.Definition.ProgressiveObject.ProgressiveDenyNode);
    }
}