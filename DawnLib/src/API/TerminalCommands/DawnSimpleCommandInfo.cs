using System;
using System.Collections.Generic;
using Dawn.Internal;

namespace Dawn;
public sealed class DawnSimpleCommandInfo
{
    public DawnTerminalCommandInfo ParentInfo { get; internal set; }

    internal DawnSimpleCommandInfo(TerminalNode resultNode)
    {
        ResultNode = resultNode;
        // normal terminalkeyword with a specialKeywordResult to a node, that's it
        // i.e. other command
    }

    internal void SetupSimpleCommand()
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
}