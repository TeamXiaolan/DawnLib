using System;
using Dawn.Interfaces;

namespace Dawn;

public static class TerminalNodeExtensions
{
    public static DawnTerminalCommandInfo GetDawnInfo(this TerminalNode terminalNode)
    {
        DawnTerminalCommandInfo terminalCommandInfo = (DawnTerminalCommandInfo)((IDawnObject)terminalNode).DawnInfo;
        return terminalCommandInfo;
    }

    internal static bool HasDawnInfo(this TerminalNode terminalNode)
    {
        return terminalNode.GetDawnInfo() != null;
    }

    internal static void SetDawnInfo(this TerminalNode terminalNode, DawnTerminalCommandInfo terminalCommandInfo)
    {
        ((IDawnObject)terminalNode).DawnInfo = terminalCommandInfo;
    }

    public static string GetDisplayText(this TerminalNode terminalNode)
    {
        if (((ITerminalNode)terminalNode).DynamicDisplayText == null)
        {
            return terminalNode.displayText;
        }
        return ((ITerminalNode)terminalNode).DynamicDisplayText.Invoke();
    }

    internal static void SetDynamicDisplayText(this TerminalNode terminalNode, Func<string> func)
    {
        ((ITerminalNode)terminalNode).DynamicDisplayText = func;
    }
}