using System;

namespace Dawn;
public sealed class DawnQueryCommandInfo
{
    public DawnTerminalCommandInfo ParentInfo { get; internal set; }

    internal DawnQueryCommandInfo(TerminalNode continueNode, TerminalNode cancelNode, Func<string> continueFunc, Func<string> cancelFunc, TerminalKeyword continueKeyword, TerminalKeyword cancelKeyword, Func<bool> continueCondition, Action<bool>? onContinuedEvent)
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

    public TerminalKeyword ContinueKeyword { get; }
    public TerminalKeyword CancelKeyword { get; }

    public Func<bool> ContinueCondition { get; private set; }
    public Action<bool>? OnContinuedEvent { get; private set; }

    public Func<string> ContinueFunc { get; private set; }
    public Func<string> CancelFunc { get; private set; }

    public TerminalNode ContinueNode { get; }
    public TerminalNode CancelNode { get; }
}