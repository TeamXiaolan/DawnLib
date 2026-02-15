using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dawn.Internal;

namespace Dawn.Utils;

public static class TerminalExtensions
{
    public static bool GetKeywordAcceptingInput(this TerminalKeyword word)
    {
        return ((ITerminalKeyword)word).DawnAcceptAdditionalText;
    }

    private static List<TerminalKeyword> WordsThatAcceptInput = new();
    public static List<TerminalKeyword> GetKeywordsAcceptingInput()
    {
        return WordsThatAcceptInput;
    }

    public static void SetKeywordAcceptInput(this TerminalKeyword word, bool value)
    {
        if (value)
        {
            WordsThatAcceptInput.Add(word);
        }
        else
        {
            WordsThatAcceptInput.Remove(word);
        }

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

    internal static void SetLastNoun(this Terminal terminal, TerminalKeyword value)
    {
        ((ITerminal)terminal).DawnLastNoun = value;
    }

    public static TerminalKeyword GetLastNoun(this Terminal terminal)
    {
        return ((ITerminal)terminal).DawnLastNoun;
    }

    internal static void SetLastVerb(this Terminal terminal, TerminalKeyword value)
    {
        ((ITerminal)terminal).DawnLastVerb = value;
    }

    public static TerminalKeyword GetLastVerb(this Terminal terminal)
    {
        return ((ITerminal)terminal).DawnLastVerb;
    }

    public static bool TryGetKeywordInfoText(this TerminalKeyword terminalKeyword, [NotNullWhen(true)] out string? text)
    {
        text = null;
        CompatibleNoun matchedCompatibleNoun = TerminalRefs.InfoKeyword.compatibleNouns.FirstOrDefault(x => x.noun.word == terminalKeyword.word);
        if (matchedCompatibleNoun == null)
        {
            return false;
        }

        text = matchedCompatibleNoun.result.displayText;
        return true;
    }

    public static bool TryGetKeyword(this Terminal terminal, string keyWord, [NotNullWhen(true)] out TerminalKeyword? terminalKeyword)
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

        terminalKeyword = null;
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
            terminalKeyword.SetKeywordPriority(ITerminalKeyword.DawnKeywordType.TerminalObject);
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

    private static readonly List<string> VanillaEvents = ["setUpTerminal", "cheat_ResetCredits", "switchCamera", "ejectPlayers"];
    //vanilla keywords that should probably not be replaced unless the API user is intending to overwrite a core function of the game
    private static readonly List<string> VanillaWords = ["company", "moons", "store", "help", "other", "bestiary", "storage", "scan", "upgrades", "decor", "sigurd"];
    public static ITerminalKeyword.DawnKeywordType TryGetTerminalNodeType(this TerminalNode terminalNode)
    {
        if (terminalNode == null)
        {
            Debuggers.Terminal?.Log("Null TerminalNode provided to TryGetTerminalNodeType, returning lowest priority");
            return ITerminalKeyword.DawnKeywordType.Other;
        }

        //vanilla terminalevents are core gameplay elements
        //modded terminalevents are usually used for custom commands, ie. LLL's simulate and WeatherRegistry's forecast
        if (!string.IsNullOrEmpty(terminalNode.terminalEvent))
        {
            if (VanillaEvents.Any(x => x.CompareStringsInvariant(terminalNode.terminalEvent)))
            {
                return ITerminalKeyword.DawnKeywordType.Core;
            }
            else
            {
                return ITerminalKeyword.DawnKeywordType.TerminalEvent;
            }
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

    public static void UpdateLastKeywordParsed(this Terminal self, TerminalKeyword terminalKeyword)
    {
        if (self == null || terminalKeyword == null)
        {
            return;
        }

        if (terminalKeyword.isVerb)
        {
            //used to get only compatible nouns of this verb
            self.SetLastVerb(terminalKeyword);
        }
        else
        {
            //usually contains a reference to the result node
            self.SetLastNoun(terminalKeyword);
        }
    }

    private static bool MatchInputKeyword(string input, out List<TerminalKeyword> words)
    {
        //get first word only
        if (input.Contains(' '))
        {
            input = input.Split(' ')[0];
        }

        words = WordsThatAcceptInput.FindAll(x => x.word.StringContainsInvariant(input));
        return words.Count != 0;
    }

    private static TerminalKeyword GetBestMatchFromList(string input, List<TerminalKeyword> keywordList)
    {
        TerminalKeyword word = null!;
        int maxScore = 0;

        //return null result from 0 matches
        if (keywordList.Count == 0)
        {
            return word;
        }

        //assign match scores for the multiple matching words
        Dictionary<TerminalKeyword, int> wordScores = [];
        foreach (TerminalKeyword keyword in keywordList)
        {
            int score = keyword.word.StringMatchScore(input);
            wordScores.TryAdd(keyword, score);
        }

        //compare each match to find the best score with the highest keyword priority
        foreach (KeyValuePair<TerminalKeyword, int> match in wordScores)
        {
            if (match.Key == null)
                continue; //skip null terminalkeywords (just in case)

            if (word == null || maxScore == 0)
            {
                word = match.Key;
                maxScore = match.Value;
                continue;
            }

            //skip since this partial match has a lower score
            if (maxScore > match.Value)
                continue;

            //this match has the same amount of matching characters
            //resolve conflict by checking keyword priority (lower number = higher priority)
            if (maxScore == match.Value)
            {
                //checks if the current match has a keyword priority value lower than the match assigned to the word variable (working match)
                //a lower keyword priority value indicates a higher priority keyword
                Debuggers.Terminal?.Log($"Attempting to resolve conflict between matching results [{word.word}] & [{match.Key.word}] by comparing keyword priorities!");
                int target = (int)word.GetKeywordPriority();
                int current = (int)match.Key.GetKeywordPriority();

                if (current < target)
                {
                    word = match.Key; //only need to update the word
                    continue;
                }
            }

            Debuggers.Terminal?.Log($"Skipping partial match [{match.Key.word}] with match score {match.Value} due to better match existing with a higher priority");
        }

        if (maxScore < DawnConfig.TerminalKeywordSpecificity.Value)
        {
            DawnPlugin.Logger.LogMessage($"GetBestMatchFromList has not found a word matching the required specificity! ({DawnConfig.TerminalKeywordSpecificity.Value})");
            return null!;
        }


        DawnPlugin.Logger.LogMessage($"GetBestMatchFromList has found match with highest priority of {word.word} ({word.GetKeywordPriority()})");
        return word;
    }

    public static bool DawnTryResolveKeyword(this Terminal terminal, string input, [NotNullWhen(true)] out TerminalKeyword? word)
    {
        word = null;

        //empty input, return false
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        //returns a non-zero list of matching keywords that accept additional input
        if (MatchInputKeyword(input, out List<TerminalKeyword> inputWords))
        {
            //return the singular matching keyword
            if (inputWords.Count == 1)
            {
                word = inputWords[0];
                return word != null;
            }
            else
            {
                //returns the best match from the list of matching keywords
                word = GetBestMatchFromList(input, inputWords);
                return word != null;
            }
        }

        //for all non-input keywords, run the vanilla method for getting a keyword
        if (!DawnConfig.TerminalKeywordResolution.Value)
            return false;

        //Now that we've checked words that accept input, check all other keywords
        List<TerminalKeyword> keywordList;
        if (terminal.GetLastVerb() != null && terminal.GetLastVerb().compatibleNouns != null && terminal.GetLastVerb().compatibleNouns.Length > 0)
        {
            //only get words that are compatible nouns to the current verb
            keywordList = [.. terminal.GetLastVerb().compatibleNouns.Select(x => x.noun)];

            //filter for our input
            keywordList = [.. keywordList.FindAll(x => x.word.StringStartsWithInvariant(input))];
        }
        else
        {
            //only get words that start with our input
            keywordList = [.. terminal.terminalNodes.allKeywords.Where(x => x.word.StringStartsWithInvariant(input))];
        }

        //if only one match exists, return it immediately
        if (keywordList.Count == 1)
        {
            word = keywordList[0];
            return word != null;
        }
        else
        {
            //returns the best match from the list of matching keywords
            word = GetBestMatchFromList(input, keywordList);
            return word != null;
        }
    }
}
