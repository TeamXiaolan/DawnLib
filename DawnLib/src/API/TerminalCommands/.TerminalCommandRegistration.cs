using Dawn.Utils;
using MonoMod.RuntimeDetour;

namespace Dawn;

//for use with creating terminal commands from plugin awake
public class TerminalCommandRegistration
{
    internal static void Init()
    {
        using (new DetourContext(priority: 0))
        {
            On.Terminal.Awake += RegisterDawnTerminalCommands;
        }
        using (new DetourContext(priority: 1))
        {
            On.Terminal.Awake += GrabVanillaTerminalCommands;
        }
        On.Terminal.LoadNewNode += HandleQueryEventAndContinueCondition;
        On.Terminal.Start += AssignTerminalPriorites;
        On.Terminal.CheckForExactSentences += CheckForExactSentencesPrefix;
        On.Terminal.ParseWord += ParseWordPrefix;
        On.Terminal.ParsePlayerSentence += HandleDawnCommand;
    }

    private static void GrabVanillaTerminalCommands(On.Terminal.orig_Awake orig, Terminal self)
    {
        // Grab the vanilla references here
        orig(self);
    }

    private static void RegisterDawnTerminalCommands(On.Terminal.orig_Awake orig, Terminal self)
    {
        foreach (DawnTerminalCommandInfo terminalCommandInfo in LethalContent.TerminalCommands.Values)
        {
            if (terminalCommandInfo.ShouldSkipIgnoreOverride())
            {
                continue;
            }

            // register the command into the terminal here.
        }
        orig(self);
    }

    private static void HandleQueryEventAndContinueCondition(On.Terminal.orig_LoadNewNode orig, Terminal self, TerminalNode node)
    {
        TerminalNode nodeToLoad = node;

        if (nodeToLoad.HasDawnInfo())
        {
            DawnTerminalCommandInfo commandInfo = nodeToLoad.GetDawnInfo();
            if (commandInfo.ResultNode == node && commandInfo.QueryCommandInfo != null)
            {
                if (!commandInfo.QueryCommandInfo.ContinueCondition.Invoke())
                {
                    nodeToLoad = commandInfo.QueryCommandInfo.CancelNode;
                    nodeToLoad.displayText = commandInfo.QueryCommandInfo.CancelFunc.Invoke();
                    commandInfo.QueryCommandInfo.OnContinuedEvent?.Invoke(false);
                }
                else
                {
                    nodeToLoad = commandInfo.QueryCommandInfo.ContinueNode;
                    nodeToLoad.displayText = commandInfo.QueryCommandInfo.ContinueFunc.Invoke();
                    commandInfo.QueryCommandInfo.OnContinuedEvent?.Invoke(true);
                }
            }
        }
        orig(self, nodeToLoad);
    }

    private static TerminalKeyword CheckForExactSentencesPrefix(On.Terminal.orig_CheckForExactSentences orig, Terminal self, string playerWord)
    {
        //reset last command values to be empty/null
        //this runs before ParseWordPrefix
        self.SetLastCommand(string.Empty);
        self.SetLastVerb(null!);
        self.SetLastNoun(null!);

        if (self.DawnTryResolveKeyword(playerWord, out var NonNullResult))
        {
            self.UpdateLastKeywordParsed(NonNullResult);
            self.SetLastCommand(playerWord.GetExactMatch(NonNullResult.word));
            return NonNullResult;
        }
        else
        {
            //run original
            TerminalKeyword vanillaResult = orig(self, playerWord);
            self.UpdateLastKeywordParsed(vanillaResult);
            self.SetLastCommand(playerWord);
            return vanillaResult;
        }

    }

    private static TerminalKeyword ParseWordPrefix(On.Terminal.orig_ParseWord orig, Terminal self, string playerWord, int specificityRequired)
    {
        if (self.DawnTryResolveKeyword(playerWord, out var NonNullResult))
        {
            self.UpdateLastKeywordParsed(NonNullResult);
            self.SetLastCommand(playerWord.GetExactMatch(NonNullResult.word));
            return NonNullResult;
        }
        else
        {
            //run original
            TerminalKeyword vanillaResult = orig(self, playerWord, specificityRequired);
            self.UpdateLastKeywordParsed(vanillaResult);
            return vanillaResult;
        }

    }

    private static void AssignTerminalPriorites(On.Terminal.orig_Start orig, Terminal self)
    {
        orig(self);

        //assign priorities to any remaining keywords that have not received a value yet
        //also assign descriptions/category if unassigned
        //doing this in start to give time after Terminal.Awake where commands are created
        foreach (TerminalKeyword keyword in self.terminalNodes.allKeywords)
        {
            keyword.TryAssignType();
            if (string.IsNullOrEmpty(keyword.GetKeywordCategory()))
            {
                keyword.SetKeywordCategory(keyword.GetKeywordPriority().ToString());
            }

            if (string.IsNullOrEmpty(keyword.GetKeywordDescription()))
            {
                if (keyword.TryGetKeywordInfoText(out var result))
                {
                    keyword.SetKeywordDescription(result.Trim());
                }
                else
                {
                    keyword.SetKeywordDescription($"No information on the terminal keyword [ {keyword.word} ]");
                }
            }
        }
    }

    private static TerminalNode HandleDawnCommand(On.Terminal.orig_ParsePlayerSentence orig, Terminal self)
    {
        //Get vanilla result
        TerminalNode terminalNode = orig(self);

        //updates the node's displaytext based on it's NodeFunction Func<string> that was injected (if not null)
        if (terminalNode.HasCommandFunction())
        {
            terminalNode.displayText = terminalNode.GetCommandFunction().Invoke();
        }

        return terminalNode;
    }
}