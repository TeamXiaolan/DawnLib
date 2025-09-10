using System.Linq;

namespace Dawn.Internal;

internal static class TerminalRefs
{
    private static Terminal _instance;
    public static Terminal Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = UnityEngine.Object.FindFirstObjectByType<Terminal>();
                BuyKeyword = _instance.terminalNodes.allKeywords.First(keyword => keyword.word == "buy");
                InfoKeyword = _instance.terminalNodes.allKeywords.First(keyword => keyword.word == "info");
                RouteKeyword = _instance.terminalNodes.allKeywords.First(keyword => keyword.word == "route");
                ConfirmKeyword = _instance.terminalNodes.allKeywords.First(keyword => keyword.word == "confirm");
                DenyKeyword = _instance.terminalNodes.allKeywords.First(keyword => keyword.word == "deny");
                CancelPurchaseNode = BuyKeyword.compatibleNouns[0].result.terminalOptions[1].result;
            }
            return _instance;
        }
    }

    public static TerminalKeyword BuyKeyword { get; private set; }
    public static TerminalKeyword InfoKeyword { get; private set; }
    public static TerminalKeyword RouteKeyword { get; private set; }
    public static TerminalKeyword ConfirmKeyword { get; private set; }
    public static TerminalKeyword DenyKeyword { get; private set; }
    public static TerminalNode CancelPurchaseNode { get; private set; }
}