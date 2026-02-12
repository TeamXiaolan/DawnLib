using System;

namespace Dawn;
public sealed class DawnQueryCommandInfo
{
    public DawnTerminalCommandInfo ParentInfo { get; internal set; }

    internal DawnQueryCommandInfo(Func<string> continueFunc, TerminalNode continueNode, TerminalNode cancelNode, Func<string> cancelFunc, TerminalKeyword continueKeyword, TerminalKeyword cancelKeyword, Func<bool> continueCondition, DawnEvent<bool>? onContinuedEvent)
    {
        ContinueNode = continueNode;
        CancelNode = cancelNode;

        ContinueFunc = continueFunc;
        CancelFunc = cancelFunc;
        ContinueKeyword = continueKeyword;
        CancelKeyword = cancelKeyword;
        ContinueCondition = continueCondition;
        OnContinuedEvent = onContinuedEvent;
    }

    internal TerminalKeyword ContinueKeyword { get; }
    internal TerminalKeyword CancelKeyword { get; }
    internal Func<bool> ContinueCondition { get; private set; }
    internal DawnEvent<bool>? OnContinuedEvent { get; private set; }

    public Func<string> ContinueFunc { get; }
    public Func<string> CancelFunc { get; }

    public TerminalNode ContinueNode { get; }
    public TerminalNode CancelNode { get; }
}