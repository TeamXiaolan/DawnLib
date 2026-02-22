namespace Dawn;
public sealed class DawnTerminalObjectCommandInfo
{
    public DawnTerminalCommandInfo ParentInfo { get; internal set; }

    internal DawnTerminalObjectCommandInfo()
    {
        // this doesnt even point to a node, i honestly don't know what to do with you until i look into hazards more.
    }

    internal void SetupTerminalObjectCommand()
    {
        foreach (TerminalKeyword commandKeyword in ParentInfo.CommandKeywords)
        {
            commandKeyword.accessTerminalObjects = true;
        }
    }

    internal void InjectCommandIntoTerminal(Terminal terminal)
    {
        TerminalKeyword[] allKeywordsModified =
        [
            .. terminal.terminalNodes.allKeywords,
            .. ParentInfo.CommandKeywords,
        ];

        terminal.terminalNodes.allKeywords = allKeywordsModified;
    }
}