using System.Collections.Generic;
using Dawn.Internal;

namespace Dawn;
public sealed class DawnComplexCommandInfo
{
    public DawnTerminalCommandInfo ParentInfo { get; internal set; }

    internal DawnComplexCommandInfo(List<TerminalNode> resultNodes, List<TerminalKeyword> secondaryTerminalKeywords)
    {
        ResultNodes = resultNodes;
        SecondaryTerminalKeywords = secondaryTerminalKeywords;
        // normal terminalkeyword with a bunch of compatiblenouns to point to different possible nodes
        // i.e. forecast none, forecast rainy
    }

    internal void SetupComplexCommand()
    {
        CompatibleNoun[] compatibleNouns = new CompatibleNoun[SecondaryTerminalKeywords.Count];

        for (int i = 0; i < SecondaryTerminalKeywords.Count; i++)
        {
            compatibleNouns[i] = new CompatibleNoun()
            {
                noun = SecondaryTerminalKeywords[i],
                result = ResultNodes[i]
            };
        }

        foreach (TerminalKeyword commandKeyword in ParentInfo.CommandKeywords)
        {
            commandKeyword.compatibleNouns = compatibleNouns;
            commandKeyword.isVerb = true;
        }

        foreach (TerminalNode resultNode in ResultNodes)
        {
            resultNode.SetDawnInfo(ParentInfo);
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

    public List<TerminalNode> ResultNodes { get; }
    public List<TerminalKeyword> SecondaryTerminalKeywords { get; }
}