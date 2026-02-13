using System.Collections.Generic;

namespace Dawn;
public sealed class DawnTerminalCommandInfo : DawnBaseInfo<DawnTerminalCommandInfo>
{
    internal DawnTerminalCommandInfo(NamespacedKey<DawnTerminalCommandInfo> key, TerminalNode resultNode, HashSet<NamespacedKey> tags, DawnComplexQueryCommandInfo? complexQueryCommandInfo, DawnSimpleQueryCommandInfo? simpleQueryCommandInfo, DawnSimpleCommandInfo? simpleCommandInfo, DawnTerminalObjectCommandInfo? terminalObjectCommandInfo, DawnEventDrivenCommandInfo? eventDrivenCommandInfo, DawnInputCommandInfo? inputCommandInfo, IDataContainer? customData) : base(key, tags, customData)
    {
        ResultNode = resultNode;

        ComplexQueryCommandInfo = complexQueryCommandInfo; // Uses a verb, similar to buy keyword/node shenanigans
        if (ComplexQueryCommandInfo != null)
        {
            ComplexQueryCommandInfo.ParentInfo = this;
        }

        SimpleQueryCommandInfo = simpleQueryCommandInfo; // Uses specialKeywordResult + overrideOptions on the node + terminalOptions
        if (SimpleQueryCommandInfo != null)
        {
            SimpleQueryCommandInfo.ParentInfo = this;
        }

        SimpleCommandInfo = simpleCommandInfo; // keyword with a special keyword result where node has nothing except text on it
        if (SimpleCommandInfo != null)
        {
            SimpleCommandInfo.ParentInfo = this;
        }

        TerminalObjectCommandInfo = terminalObjectCommandInfo; // this doesn't even have a node to it, it's just a checkbox
        if (TerminalObjectCommandInfo != null)
        {
            TerminalObjectCommandInfo.ParentInfo = this;
        }

        EventDrivenCommandInfo = eventDrivenCommandInfo; // TerminalKeyword pointing to a TerminalNode that contains a TerminalEvent
        if (EventDrivenCommandInfo != null)
        {
            EventDrivenCommandInfo.ParentInfo = this;
        }

        InputCommandInfo = inputCommandInfo;
        if (InputCommandInfo != null)
        {
            InputCommandInfo.ParentInfo = this;
        }
    }

    public TerminalNode ResultNode { get; }

    public DawnComplexQueryCommandInfo? ComplexQueryCommandInfo { get; }
    public DawnSimpleQueryCommandInfo? SimpleQueryCommandInfo { get; }
    public DawnSimpleCommandInfo? SimpleCommandInfo { get; }
    public DawnTerminalObjectCommandInfo? TerminalObjectCommandInfo { get; }
    public DawnEventDrivenCommandInfo? EventDrivenCommandInfo { get; }
    public DawnInputCommandInfo? InputCommandInfo { get; }
}