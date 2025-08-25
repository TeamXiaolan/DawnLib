using Unity.Collections;

namespace CodeRebirthLib;
public abstract class TerminalPurchaseResult
{
    public static TerminalPurchaseResult Success()
    {
        return SuccessPurchaseResult.Instance;
    }

    public static TerminalPurchaseResult Fail(TerminalNode node, string? overrideName = null)
    {
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
}