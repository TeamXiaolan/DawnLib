using System;

namespace Dawn;
public sealed class DawnEventDrivenCommandInfo
{
    public DawnTerminalCommandInfo ParentInfo { get; internal set; }

    internal DawnEventDrivenCommandInfo()
    {
        // just a terminalkeyword with a specialkeywordresult for a node
        // the node needs to have a terminalEvent thingy setup for it
        // look into all vanilla "terminalEvent"'s and rewrite them to be event based and rewrite the RunTerminalEvents for vanilla.
        // i.e. switch command
    }
}