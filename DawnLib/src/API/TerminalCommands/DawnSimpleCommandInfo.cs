using System;

namespace Dawn;
public sealed class DawnSimpleCommandInfo
{
    public DawnTerminalCommandInfo ParentInfo { get; internal set; }

    internal DawnSimpleCommandInfo()
    {
        // normal terminalkeyword with a specialKeywordResult to a node, that's it
        // i.e. other command
    }
}