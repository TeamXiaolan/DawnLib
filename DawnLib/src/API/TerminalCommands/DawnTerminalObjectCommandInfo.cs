using System;

namespace Dawn;
public sealed class DawnTerminalObjectCommandInfo
{
    public DawnTerminalCommandInfo ParentInfo { get; internal set; }

    internal DawnTerminalObjectCommandInfo()
    {
        // this doesnt even point to a node, i honestly don't know what to do with you until i look into hazards more.
    }
}