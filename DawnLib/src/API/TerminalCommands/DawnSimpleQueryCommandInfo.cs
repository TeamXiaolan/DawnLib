using System;

namespace Dawn;
public sealed class DawnSimpleQueryCommandInfo
{
    public DawnTerminalCommandInfo ParentInfo { get; internal set; }

    internal DawnSimpleQueryCommandInfo(TerminalNode resultNode, TerminalNode continueOrCancelNode, TerminalNode cancelNode, TerminalKeyword continueKeyword, TerminalKeyword cancelKeyword, Func<bool> continueCondition, Action<bool> onContinuedEvent)
    {
        // keyword with a specialkeywordresult
        // that specialkeywordresult
        ResultNode = resultNode;
        ContinueOrCancelNode = continueOrCancelNode;
        CancelNode = cancelNode;

        ContinueKeyword = continueKeyword;
        CancelKeyword = cancelKeyword;

        ContinueCondition = continueCondition;
        OnContinuedEvent = onContinuedEvent;
    }

    internal void SetupSimpleQueryCommand()
    {
        foreach (TerminalKeyword commandKeyword in ParentInfo.CommandKeywords)
        {
            commandKeyword.specialKeywordResult = ContinueOrCancelNode;
        }

        ContinueOrCancelNode.overrideOptions = true;
        ContinueOrCancelNode.terminalOptions =
        [
            new()
            {
                noun = ContinueKeyword,
                result = ResultNode
            },
            new()
            {
                noun = CancelKeyword,
                result = CancelNode
            }
        ];

        ResultNode.SetDawnInfo(ParentInfo);
    }

    internal void InjectCommandIntoTerminal(Terminal terminal)
    {
        TerminalKeyword[] allKeywordsModified =
        [
            .. terminal.terminalNodes.allKeywords,
            .. ParentInfo.CommandKeywords,
            ContinueKeyword,
            CancelKeyword
        ];

        terminal.terminalNodes.allKeywords = allKeywordsModified;
    }

    public TerminalKeyword ContinueKeyword { get; }
    public TerminalKeyword CancelKeyword { get; }

    public Func<bool> ContinueCondition { get; private set; }
    public Action<bool> OnContinuedEvent { get; private set; }

    public TerminalNode ResultNode { get; }
    public TerminalNode ContinueOrCancelNode { get; }
    public TerminalNode CancelNode { get; }
}