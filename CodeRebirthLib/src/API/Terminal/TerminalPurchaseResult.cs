using Unity.Collections;

namespace CodeRebirthLib;
public abstract class TerminalPurchaseResult
{
    public static TerminalPurchaseResult Success()
    {
        return SuccessPurchaseResult.Instance;
    }

    public static TerminalPurchaseResult Fail(TerminalNode node)
    {
        return new FailedPurchaseResult(node);
    }

    public class SuccessPurchaseResult : TerminalPurchaseResult
    {
        internal static SuccessPurchaseResult Instance { get; } = new SuccessPurchaseResult();
        private SuccessPurchaseResult() { }
    }

    public class FailedPurchaseResult : TerminalPurchaseResult
    {
        internal FailedPurchaseResult(TerminalNode node)
        {
            ReasonNode = node;
        }
        public TerminalNode ReasonNode { get; }
    }
}