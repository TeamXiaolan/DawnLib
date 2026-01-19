using System;

namespace Dawn;
public sealed class DawnQueryCommandInfo
{
    public DawnTerminalCommandInfo ParentInfo { get; internal set; }

    internal DawnQueryCommandInfo(Func<string> queryFunc, Func<string> cancelFunc, string continueKeyword, string cancelKeyword)
    {
        QueryFunc = queryFunc;
        CancelFunc = cancelFunc;
        ContinueKeyword = continueKeyword;
        CancelKeyword = cancelKeyword;
    }

    internal Func<string> QueryFunc { get; }
    internal Func<string> CancelFunc { get; }
    internal string ContinueKeyword { get; }
    internal string CancelKeyword { get; }

    public TerminalNode? QueryNode { get; internal set; }
    public TerminalNode? CancelNode { get; internal set; }
}