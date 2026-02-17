using System;
using System.Collections.Generic;

namespace Dawn;
public sealed class DawnComplexQueryCommandInfo
{
    public DawnTerminalCommandInfo ParentInfo { get; internal set; }

    internal DawnComplexQueryCommandInfo(List<TerminalNode> resultNodes, TerminalNode continueOrCancelNode, TerminalNode? cancelNode, List<TerminalKeyword> continueKeywords, TerminalKeyword? cancelKeyword, List<Func<bool>> continueConditions, List<Action<bool>> onContinuedEvents)
    {
        // keyword with a specialkeywordresult
        // that specialkeywordresult has a bunch of compatible nouns for this
        ResultNodes = resultNodes;
        ContinueOrCancelNode = continueOrCancelNode;
        CancelNode = cancelNode;

        ContinueKeywords = continueKeywords;
        CancelKeyword = cancelKeyword;

        ContinueConditions = continueConditions;
        OnContinuedEvents = onContinuedEvents;
    }

    internal void SetupComplexQueryCommand()
    {
        foreach (TerminalKeyword commandKeyword in ParentInfo.CommandKeywords)
        {
            commandKeyword.specialKeywordResult = ContinueOrCancelNode;
        }

        ContinueOrCancelNode.overrideOptions = true;
        CompatibleNoun[] compatibleNouns = new CompatibleNoun[ContinueKeywords.Count + (CancelKeyword != null ? 1 : 0)];

        for (int i = 0; i < ContinueKeywords.Count; i++)
        {
            compatibleNouns[i] = new CompatibleNoun()
            {
                noun = ContinueKeywords[i],
                result = ResultNodes[i]
            };
        }

        if (CancelKeyword != null)
        {
            compatibleNouns[^1] = new CompatibleNoun()
            {
                noun = CancelKeyword,
                result = CancelNode
            };
        }

        ContinueOrCancelNode.terminalOptions = compatibleNouns;

        foreach (TerminalNode resultNode in ResultNodes)
        {
            resultNode.SetDawnInfo(ParentInfo);
        }
    }

    internal void InjectCommandIntoTerminal(Terminal terminal)
    {
        List<TerminalKeyword> allKeywordsModified =
        [
            .. terminal.terminalNodes.allKeywords,
            .. ParentInfo.CommandKeywords,
            .. ContinueKeywords,
        ];

        if (CancelKeyword != null)
        {
            allKeywordsModified.Add(CancelKeyword);
        }

        terminal.terminalNodes.allKeywords = allKeywordsModified.ToArray();
    }

    public List<TerminalKeyword> ContinueKeywords { get; }
    public TerminalKeyword? CancelKeyword { get; }

    public List<Func<bool>> ContinueConditions { get; private set; }
    public List<Action<bool>> OnContinuedEvents { get; private set; }

    public List<TerminalNode> ResultNodes { get; }
    public TerminalNode ContinueOrCancelNode { get; }
    public TerminalNode? CancelNode { get; }
}