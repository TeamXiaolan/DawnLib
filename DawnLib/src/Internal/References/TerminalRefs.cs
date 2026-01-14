using System.Linq;

namespace Dawn.Internal;

public static class TerminalRefs
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
                ViewKeyword = _instance.terminalNodes.allKeywords.First(keyword => keyword.word == "view");
                ConfirmPurchaseKeyword = _instance.terminalNodes.allKeywords.First(keyword => keyword.word == "confirm");
                DenyKeyword = _instance.terminalNodes.allKeywords.First(keyword => keyword.word == "deny");
                MoonsKeyword = _instance.terminalNodes.allKeywords.First(keyword => keyword.word == "moons");
                CancelPurchaseNode = BuyKeyword.compatibleNouns[0].result.terminalOptions[1].result;
                CancelRouteNode = RouteKeyword.compatibleNouns[0].result.terminalOptions[0].result;
                MoonCatalogueNode = MoonsKeyword.specialKeywordResult;
            }
            return _instance;
        }
    }

    public static TerminalKeyword BuyKeyword { get; private set; }
    public static TerminalKeyword InfoKeyword { get; private set; }
    public static TerminalKeyword RouteKeyword { get; private set; }
    public static TerminalKeyword ConfirmPurchaseKeyword { get; private set; }
    public static TerminalKeyword DenyKeyword { get; private set; }
    public static TerminalKeyword ViewKeyword { get; internal set; }
    public static TerminalKeyword MoonsKeyword { get; private set; }
    public static TerminalNode CancelPurchaseNode { get; private set; }
    public static TerminalNode CancelRouteNode { get; private set; }
    public static TerminalNode MoonCatalogueNode { get; private set; }
    public static int LastVehicleDelivered { get; internal set; } = -1;
}