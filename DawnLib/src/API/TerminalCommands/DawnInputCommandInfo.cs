using System;
using Dawn.Internal;
using Dawn.Utils;

namespace Dawn;
public sealed class DawnInputCommandInfo
{
    public DawnTerminalCommandInfo ParentInfo { get; internal set; }

    internal DawnInputCommandInfo(TerminalNode resultNode, Func<string, string> dynamicInputTextResult)
    {
        ResultNode = resultNode;
        DynamicInputTextResult = dynamicInputTextResult;
        // normal terminalkeyword into a specialkeywordresult node
        // allow accepting additional text
        // feed the result node the input in some way so that the Func<string> main display can be edited by the input.
        // i.e. input command in DawnTesting
    }

    internal void SetupInputCommand()
    {
        foreach (TerminalKeyword commandKeyword in ParentInfo.CommandKeywords)
        {
            commandKeyword.specialKeywordResult = ResultNode;
            commandKeyword.SetKeywordAcceptInput(true);
        }

        ResultNode.SetDawnInfo(ParentInfo);
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

    public static string GetLastUserInput()
    {
        if (TerminalRefs.Instance == null)
        {
            return "Terminal not Initialized yet";
        }

        string cleanedText = TerminalRefs.Instance.screenText.text[^TerminalRefs.Instance.textAdded..];
        if (string.IsNullOrEmpty(TerminalRefs.Instance.GetLastCommand()))
        {
            cleanedText = string.Empty;
        }
        else
        {
            cleanedText = cleanedText.Replace(TerminalRefs.Instance.GetLastCommand(), "").Trim();
        }
        return cleanedText;
    }

    public Func<string, string> DynamicInputTextResult { get; }
    public TerminalNode ResultNode { get; }
    public TerminalKeyword InputKeyword { get; }
}