using System.Collections.Generic;

namespace Dawn;
public sealed class DawnTerminalCommandInfo : DawnBaseInfo<DawnTerminalCommandInfo>
{
    internal DawnTerminalCommandInfo(NamespacedKey<DawnTerminalCommandInfo> key, TerminalCommandBasicInformation commandBasicInformation, List<TerminalKeyword> commandKeywords, bool buildOnTerminalAwake, HashSet<NamespacedKey> tags, DawnComplexQueryCommandInfo? complexQueryCommandInfo, DawnSimpleQueryCommandInfo? simpleQueryCommandInfo, DawnComplexCommandInfo? complexCommandInfo, DawnSimpleCommandInfo? simpleCommandInfo, DawnTerminalObjectCommandInfo? terminalObjectCommandInfo, DawnEventDrivenCommandInfo? eventDrivenCommandInfo, DawnInputCommandInfo? inputCommandInfo, IDataContainer? customData) : base(key, tags, customData)
    {
        CommandBasicInformation = commandBasicInformation;
        CommandKeywords = commandKeywords;
        BuildOnTerminalAwake = buildOnTerminalAwake;

        ComplexQueryCommandInfo = complexQueryCommandInfo; // Uses a verb, similar to buy keyword/node shenanigans
        if (ComplexQueryCommandInfo != null)
        {
            ComplexQueryCommandInfo.ParentInfo = this;
            if (!ShouldSkipIgnoreOverride())
            {
                ComplexQueryCommandInfo.SetupComplexQueryCommand();
            }
        }

        SimpleQueryCommandInfo = simpleQueryCommandInfo; // Uses specialKeywordResult + overrideOptions on the node + terminalOptions
        if (SimpleQueryCommandInfo != null)
        {
            SimpleQueryCommandInfo.ParentInfo = this;
            if (!ShouldSkipIgnoreOverride())
            {
                SimpleQueryCommandInfo.SetupSimpleQueryCommand();
            }
        }

        ComplexCommandInfo = complexCommandInfo;
        if (ComplexCommandInfo != null)
        {
            ComplexCommandInfo.ParentInfo = this;
            if (!ShouldSkipIgnoreOverride())
            {
                ComplexCommandInfo.SetupComplexCommand();
            }
        }

        SimpleCommandInfo = simpleCommandInfo; // keyword with a special keyword result where node has nothing except text on it
        if (SimpleCommandInfo != null)
        {
            SimpleCommandInfo.ParentInfo = this;
            if (!ShouldSkipIgnoreOverride())
            {
                SimpleCommandInfo.SetupSimpleCommand();
            }
        }

        TerminalObjectCommandInfo = terminalObjectCommandInfo; // this doesn't even have a node to it, it's just a checkbox
        if (TerminalObjectCommandInfo != null)
        {
            TerminalObjectCommandInfo.ParentInfo = this;
            if (!ShouldSkipIgnoreOverride())
            {
                TerminalObjectCommandInfo.SetupTerminalObjectCommand();
            }
        }

        EventDrivenCommandInfo = eventDrivenCommandInfo; // TerminalKeyword pointing to a TerminalNode that contains a TerminalEvent
        if (EventDrivenCommandInfo != null)
        {
            EventDrivenCommandInfo.ParentInfo = this;
            if (!ShouldSkipIgnoreOverride())
            {
                EventDrivenCommandInfo.SetupEventDrivenCommand();
            }
        }

        InputCommandInfo = inputCommandInfo;
        if (InputCommandInfo != null)
        {
            InputCommandInfo.ParentInfo = this;
            if (!ShouldSkipIgnoreOverride())
            {
                InputCommandInfo.SetupInputCommand();
            }
        }
    }

    public void InjectCommandIntoTerminal(Terminal terminal)
    {
        if (CommandInjected)
        {
            DawnPlugin.Logger.LogWarning($"Command {Key} already injected into terminal, yet tried to inject again.");
            return;
        }

        CommandInjected = true;
        if (ComplexQueryCommandInfo != null)
        {
            ComplexQueryCommandInfo.InjectCommandIntoTerminal(terminal);
        }

        if (SimpleQueryCommandInfo != null)
        {
            SimpleQueryCommandInfo.InjectCommandIntoTerminal(terminal);
        }

        if (ComplexCommandInfo != null)
        {
            ComplexCommandInfo.InjectCommandIntoTerminal(terminal);
        }

        if (SimpleCommandInfo != null)
        {
            SimpleCommandInfo.InjectCommandIntoTerminal(terminal);
        }

        if (TerminalObjectCommandInfo != null)
        {
            TerminalObjectCommandInfo.InjectCommandIntoTerminal(terminal);
        }

        if (EventDrivenCommandInfo != null)
        {
            EventDrivenCommandInfo.InjectCommandIntoTerminal(terminal);
        }

        if (InputCommandInfo != null)
        {
            InputCommandInfo.InjectCommandIntoTerminal(terminal);
        }
    }

    public TerminalCommandBasicInformation CommandBasicInformation { get; }
    public List<TerminalKeyword> CommandKeywords { get; }
    public bool BuildOnTerminalAwake { get; }

    public DawnComplexQueryCommandInfo? ComplexQueryCommandInfo { get; }
    public DawnSimpleQueryCommandInfo? SimpleQueryCommandInfo { get; }
    public DawnComplexCommandInfo? ComplexCommandInfo { get; }
    public DawnSimpleCommandInfo? SimpleCommandInfo { get; }
    public DawnTerminalObjectCommandInfo? TerminalObjectCommandInfo { get; }
    public DawnEventDrivenCommandInfo? EventDrivenCommandInfo { get; }
    public DawnInputCommandInfo? InputCommandInfo { get; }

    public bool CommandInjected { get; private set; }
}