namespace Dawn;
public abstract class TerminalPurchaseResult
{
    public static SuccessPurchaseResult Success()
    {
        return SuccessPurchaseResult.Instance;
    }

    public static HiddenPurchaseResult Hidden()
    {
        return HiddenPurchaseResult.Instance;
    }

    public static FailedPurchaseResult Fail(TerminalNode node, string? overrideName = null)
    {
        // if string is "" force it to be null
        if (string.IsNullOrEmpty(overrideName)) overrideName = null;
        return new FailedPurchaseResult(node, overrideName);
    }

    public class SuccessPurchaseResult : TerminalPurchaseResult
    {
        internal static SuccessPurchaseResult Instance { get; } = new SuccessPurchaseResult();
        private SuccessPurchaseResult() { }
    }

    public class FailedPurchaseResult : TerminalPurchaseResult
    {
        internal FailedPurchaseResult(TerminalNode node, string? overrideName)
        {
            ReasonNode = node;
            OverrideName = overrideName;
        }

        public string? OverrideName { get; }
        public TerminalNode ReasonNode { get; }
    }

    public class HiddenPurchaseResult : TerminalPurchaseResult
    {
        internal static HiddenPurchaseResult Instance { get; } = new HiddenPurchaseResult();
        private HiddenPurchaseResult() { }
    }
}