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

    public static Func<string> GetCommandFunction(this TerminalNode node)
    {
        return ((ITerminalNode)node).DawnNodeFunction;
    }

    internal static bool HasCommandFunction(this TerminalNode node)
    {
        if (node == null)
        {
            return false;
        }

        return node.GetCommandFunction() != null;
    }

    internal static void SetNodeFunction(this TerminalNode node, Func<string> NodeFunc)
    {
        ((ITerminalNode)node).DawnNodeFunction = NodeFunc;
    }
}
