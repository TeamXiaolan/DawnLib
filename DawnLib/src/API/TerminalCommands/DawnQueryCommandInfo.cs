using System;

namespace Dawn;
public sealed class DawnQueryCommandInfo
{
    public DawnTerminalCommandInfo ParentInfo { get; internal set; }

    internal DawnQueryCommandInfo(Func<string> queryFunc, Func<string> cancelFunc, string continueKeyword, string cancelKeyword, FuncProvider<bool> continueProvider, DawnEvent<bool>? onQueryContinuedEvent)
    {
        QueryFunc = queryFunc;
        CancelFunc = cancelFunc;
        ContinueKeyword = continueKeyword;
        CancelKeyword = cancelKeyword;
        ContinueProvider = continueProvider;
        OnQueryContinuedEvent = onQueryContinuedEvent;
    }

    internal Func<string> QueryFunc { get; }
    internal Func<string> CancelFunc { get; }
    internal string ContinueKeyword { get; }
    internal string CancelKeyword { get; }
    internal FuncProvider<bool> ContinueProvider { get; private set; }
    internal DawnEvent<bool>? OnQueryContinuedEvent { get; private set; }

    public TerminalNode? QueryNode { get; internal set; }
    public TerminalNode? CancelNode { get; internal set; }
}