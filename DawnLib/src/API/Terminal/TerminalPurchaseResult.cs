
namespace Dawn;
public abstract class TerminalPurchaseResult
{
    public static SuccessPurchaseResult Success()
    {
        return SuccessPurchaseResult.Instance;
    }

    public static HiddenPurchaseResult Hidden()
    {
        return new HiddenPurchaseResult();
    }

    public static FailedPurchaseResult Fail(TerminalNode node)
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

        public string? OverrideName { get; private set; } = null;
        public TerminalNode ReasonNode { get; }

        public FailedPurchaseResult SetOverrideName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                // don't update it
                return this;
            }
            OverrideName = name;
            return this;
        }
    }

    public class HiddenPurchaseResult : TerminalPurchaseResult
    {
        public bool IsFailure { get; private set; } = true;
        public TerminalNode ReasonNode { get; private set; }

        private static TerminalNode _defaultNode = new TerminalNodeBuilder($"{nameof(HiddenPurchaseResult)}")
                .SetDisplayText("This content is hidden and made inaccessible.")
                .Build();

        internal HiddenPurchaseResult()
        {
            ReasonNode = _defaultNode;
        }

        public HiddenPurchaseResult SetFailure(bool isFailure)
        {
            IsFailure = isFailure;
            return this;
        }

        public HiddenPurchaseResult SetFailNode(TerminalNode terminalNode)
        {
            ReasonNode = terminalNode;
            return this;
        }
    }
}