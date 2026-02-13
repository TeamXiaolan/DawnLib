using System;

namespace Dawn;
public sealed class DawnInputCommandInfo
{
    public DawnTerminalCommandInfo ParentInfo { get; internal set; }

    internal DawnInputCommandInfo()
    {
        // normal terminalkeyword into a specialkeywordresult node
        // allow accepting additional text
        // feed the result node the input in some way so that the Func<string> main display can be edited by the input.
        // i.e. input command in DawnTesting
    }
}