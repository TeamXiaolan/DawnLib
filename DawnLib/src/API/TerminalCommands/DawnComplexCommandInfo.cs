using System;

namespace Dawn;
public sealed class DawnComplexCommandInfo
{
    public DawnTerminalCommandInfo ParentInfo { get; internal set; }

    internal DawnComplexCommandInfo()
    {
        // normal terminalkeyword with a bunch of compatiblenouns to point to different possible nodes
        // i.e. forecast none, forecast rainy
    }
}