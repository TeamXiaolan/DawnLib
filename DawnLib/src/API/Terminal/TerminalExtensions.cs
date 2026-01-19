using System;
using System.Collections.Generic;
using System.Linq;
using Dawn.Internal;
using Dawn.Utils;

namespace Dawn;

public static class TerminalExtensions
{
    public static Func<string> GetCommandFunction(this TerminalNode node)
    {
        return ((ITerminalNode)node).DawnNodeFunction;
    }

    internal static bool HasCommandFunction(this TerminalNode node)
    {
        if (node == null)
        {
            return false;
        }

        return node.GetCommandFunction() != null;
    }

    internal static void SetNodeFunction(this TerminalNode node, Func<string> NodeFunc)
    {
        ((ITerminalNode)node).DawnNodeFunction = NodeFunc;
    }

    public static bool GetKeywordAcceptInput(this TerminalKeyword word)
    {
        return ((ITerminalKeyword)word).DawnAcceptAdditionalText;
    }

    public static void SetKeywordAcceptInput(this TerminalKeyword word, bool value)
    {
        ((ITerminalKeyword)word).DawnAcceptAdditionalText = value;
    }

    public static string GetKeywordCategory(this TerminalKeyword word)
    {
        return ((ITerminalKeyword)word).DawnKeywordCategory;
    }

    public static void SetKeywordCategory(this TerminalKeyword word, string value)
    {
        ((ITerminalKeyword)word).DawnKeywordCategory = value;
    }

    public static string GetKeywordDescription(this TerminalKeyword word)
    {
        return ((ITerminalKeyword)word).DawnKeywordDescription;
    }

    public static void SetKeywordDescription(this TerminalKeyword word, string value)
    {
        ((ITerminalKeyword)word).DawnKeywordDescription = value;
    }

    internal static void SetLastCommand(this Terminal terminal, string value)
    {
        ((ITerminal)terminal).DawnLastCommand = value;
    }

    public static string GetLastCommand(this Terminal terminal)
    {
        return ((ITerminal)terminal).DawnLastCommand;
    }

    public static bool TryGetKeywordInfoText(this TerminalKeyword terminalKeyword, out string text)
    {
        text = string.Empty;
        CompatibleNoun matchedCompatibleNoun = TerminalRefs.InfoKeyword.compatibleNouns.FirstOrDefault(x => x.noun.word == terminalKeyword.word);
        if (matchedCompatibleNoun == null)
        {
            return false;
        }

        text = matchedCompatibleNoun.result.displayText;
        return true;
    }

    public static bool TryGetKeyword(this Terminal terminal, string keyWord, out TerminalKeyword terminalKeyword)
    {
        List<TerminalKeyword> keyWordList = [.. terminal.terminalNodes.allKeywords];

        foreach (TerminalKeyword keyword in keyWordList)
        {
            if (keyWord.CompareStringsInvariant(keyword.word))
            {
                //Loggers.LogDebug($"Keyword: [{keyWord}] found!");
                terminalKeyword = keyword;
                return true;
            }
        }

        terminalKeyword = null!;
        return false;
    }

    internal static void TryAssignType(this TerminalKeyword terminalKeyword)
    {
        if (terminalKeyword.GetKeywordPriority() != 0)
            return;

        if (terminalKeyword.isVerb || VanillaWords.Contains(terminalKeyword.word.ToLowerInvariant()))
        {
            terminalKeyword.SetKeywordPriority(ITerminalKeyword.DawnKeywordType.Core);
            return;
        }

        if (terminalKeyword.accessTerminalObjects)
        {
            terminalKeyword.SetKeywordPriority(ITerminalKeyword.DawnKeywordType.Code);
            return;
        }

        if (terminalKeyword.specialKeywordResult != null)
        {
            terminalKeyword.SetKeywordPriority(terminalKeyword.specialKeywordResult.TryGetTerminalNodeType());
            return;
        }

        if (terminalKeyword.defaultVerb != null)
        {
            CompatibleNoun matchedCompatibleNoun = terminalKeyword.defaultVerb.compatibleNouns.FirstOrDefault(x => x.noun.word.CompareStringsInvariant(terminalKeyword.word));
            if (matchedCompatibleNoun != null)
            {
                ITerminalKeyword.DawnKeywordType priority = matchedCompatibleNoun.result.TryGetTerminalNodeType();
                terminalKeyword.SetKeywordPriority(priority);
                Debuggers.Terminal?.Log($"{terminalKeyword.word} priority set to {priority}");
                return;
            }
            else
            {
                Debuggers.Terminal?.Log($"Unable to determine keyword type for word: [ {terminalKeyword.word} ]\nKeywordPriority is set to other!");
            }
        }

        terminalKeyword.SetKeywordPriority(ITerminalKeyword.DawnKeywordType.Other);
    }

    //vanilla keywords that should probably not be replaced unless the API user is intending to overwrite a core function of the game
    private static readonly List<string> VanillaWords = ["company", "moons", "store", "help", "other", "bestiary", "storage", "scan", "upgrades", "decor", "sigurd"];
    public static ITerminalKeyword.DawnKeywordType TryGetTerminalNodeType(this TerminalNode terminalNode)
    {
        if (terminalNode == null)
        {
            Debuggers.Terminal?.Log("Null TerminalNode provided to TryGetTerminalNodeType, returning lowest priority");
            return ITerminalKeyword.DawnKeywordType.Other;
        }

        //just assuming any node with a terminal event string is a core gameplay element
        //vanilla examples are eject & switch
        if (!string.IsNullOrEmpty(terminalNode.terminalEvent))
        {
            return ITerminalKeyword.DawnKeywordType.Core;
        }

        //moon keywords
        if (terminalNode.buyRerouteToMoon > -1 || terminalNode.displayPlanetInfo > -1)
        {
            return ITerminalKeyword.DawnKeywordType.Moons;
        }

        //vehicle keywords
        if (terminalNode.buyVehicleIndex > -1)
        {
            return ITerminalKeyword.DawnKeywordType.Vehicles;
        }

        //shop keywords
        if (terminalNode.shipUnlockableID > -1 || terminalNode.buyItemIndex > -1)
        {
            return ITerminalKeyword.DawnKeywordType.Store;
        }

        //bestiary keywords
        if (terminalNode.creatureFileID > -1)
        {
            return ITerminalKeyword.DawnKeywordType.Bestiary;
        }

        //log keywords
        if (terminalNode.storyLogFileID > -1)
        {
            return ITerminalKeyword.DawnKeywordType.SigurdLog;
        }

        //command keywords
        if (terminalNode.HasCommandFunction())
        {
            return ITerminalKeyword.DawnKeywordType.DawnCommand;
        }

        //no matching types
        return ITerminalKeyword.DawnKeywordType.Other;
    }

    public static ITerminalKeyword.DawnKeywordType GetKeywordPriority(this TerminalKeyword terminalKeyword)
    {
        return ((ITerminalKeyword)terminalKeyword).DawnKeywordPriority;
    }

    public static void SetKeywordPriority(this TerminalKeyword terminalKeyword, ITerminalKeyword.DawnKeywordType keywordType)
    {
        ((ITerminalKeyword)terminalKeyword).DawnKeywordPriority = keywordType;
    }
}
