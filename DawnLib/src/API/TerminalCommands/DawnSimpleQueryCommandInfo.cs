using System;

namespace Dawn;
public sealed class DawnSimpleQueryCommandInfo
{
    public DawnTerminalCommandInfo ParentInfo { get; internal set; }

    internal DawnSimpleQueryCommandInfo(TerminalNode continueNode, TerminalNode cancelNode, TerminalKeyword continueKeyword, TerminalKeyword cancelKeyword, Func<bool> continueCondition, Action<bool>? onContinuedEvent)
    {
        ContinueNode = continueNode;
        CancelNode = cancelNode;

        ContinueKeyword = continueKeyword;
        CancelKeyword = cancelKeyword;

        ContinueCondition = continueCondition;
        OnContinuedEvent = onContinuedEvent;
    }

    public TerminalKeyword ContinueKeyword { get; }
    public TerminalKeyword CancelKeyword { get; }

    public Func<bool> ContinueCondition { get; private set; }
    public Action<bool>? OnContinuedEvent { get; private set; }

    public TerminalNode ContinueNode { get; }
    public TerminalNode CancelNode { get; }
}