using System;

namespace Dawn;
public sealed class DawnEventDrivenCommandInfo
{
    public DawnTerminalCommandInfo ParentInfo { get; internal set; }

    internal DawnEventDrivenCommandInfo(TerminalNode resultNode, Action<Terminal, TerminalNode> onTerminalEvent)
    {
        ResultNode = resultNode;
        OnTerminalEvent = onTerminalEvent;
        // just a terminalkeyword with a specialkeywordresult for a node
        // the node needs to have a terminalEvent thingy setup for it
        // look into all vanilla "terminalEvent"'s and rewrite them to be event based and rewrite the RunTerminalEvents for vanilla.
        // i.e. switch command
    }

    internal void SetupEventDrivenCommand()
    {
        foreach (TerminalKeyword commandKeyword in ParentInfo.CommandKeywords)
        {
            commandKeyword.specialKeywordResult = ResultNode;
        }

        ResultNode.SetDawnInfo(ParentInfo);
    }

    internal void InjectCommandIntoTerminal(Terminal terminal)
    {
        TerminalKeyword[] allKeywordsModified =
        [
            .. terminal.terminalNodes.allKeywords,
            .. ParentInfo.CommandKeywords,
        ];

        terminal.terminalNodes.allKeywords = allKeywordsModified;
    }

    public TerminalNode ResultNode { get; }
    public Action<Terminal, TerminalNode> OnTerminalEvent { get; }
}