using System;
using System.Collections.Generic;
using Dawn.Utils;
using HarmonyLib;
using MonoMod.RuntimeDetour;

namespace Dawn;

[HarmonyPatch]
static class TerminalCommandRegistration
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

        using (new DetourContext(priority: -9999))
        {
            On.Terminal.LoadNewNode += AssignNodeProperDisplayText;
        }
        On.Terminal.LoadNewNode += HandleQueryEventAndContinueCondition;
        On.Terminal.Start += AssignTerminalPriorites;
        On.Terminal.CheckForExactSentences += CheckForExactSentencesPrefix;
        On.Terminal.ParseWord += ParseWordPrefix;
    }

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.RunTerminalEvents)), HarmonyPrefix, HarmonyPriority(int.MaxValue)]
    static bool OverrideVanillaTerminalsAndRunDawnEvents(Terminal __instance, TerminalNode node)
    {
        if (node.HasDawnInfo())
        {
            DawnEventDrivenCommandInfo? eventDrivenCommandInfo = node.GetDawnInfo().EventDrivenCommandInfo;
            if (eventDrivenCommandInfo != null)
            {
                eventDrivenCommandInfo.OnTerminalEvent(__instance, node);
            }
        }
        return false;
    }

    private static void AssignNodeProperDisplayText(On.Terminal.orig_LoadNewNode orig, Terminal self, TerminalNode node)
    {
        node.displayText = node.GetDisplayText();
        orig(self, node);
    }

    private static void GrabVanillaTerminalCommands(On.Terminal.orig_Awake orig, Terminal self)
    {
        if (LethalContent.TerminalCommands.IsFrozen)
        {
            orig(self);
            return;
        }

        foreach (TerminalKeyword terminalKeyword in self.terminalNodes.allKeywords)
        {
            if (terminalKeyword.specialKeywordResult == null)
                continue;

            List<TerminalNode> terminalNodesToCheck = [terminalKeyword.specialKeywordResult];
            if (terminalKeyword.specialKeywordResult.terminalOptions != null)
            {
                foreach (CompatibleNoun compatibleNoun in terminalKeyword.specialKeywordResult.terminalOptions)
                {
                    if (compatibleNoun.result == null)
                        continue;

                    terminalNodesToCheck.Add(compatibleNoun.result);
                }
            }

            foreach (TerminalNode nodeToCheck in terminalNodesToCheck)
            {
                if (nodeToCheck.HasDawnInfo() || string.IsNullOrEmpty(nodeToCheck.terminalEvent))
                    continue;

                Action<Terminal, TerminalNode>? onTerminalEvent = null;
                switch (nodeToCheck.terminalEvent)
                {
                    case "switchCamera":
                        onTerminalEvent = VanillaTerminalEvents.SwitchCameraEvent;
                        break;
                    case "setUpTerminal":
                        onTerminalEvent = VanillaTerminalEvents.SetUpTerminalEvent;
                        break;
                    case "ejectPlayers":
                        onTerminalEvent = VanillaTerminalEvents.EjectPlayersEvent;
                        break;
                    case "cheat_ResetCredits":
                        onTerminalEvent = VanillaTerminalEvents.Cheat_ResetCreditsEvent;
                        break;
                }

                if (onTerminalEvent == null)
                    continue;

                nodeToCheck.terminalEvent = string.Empty;
                NamespacedKey<DawnTerminalCommandInfo> namespacedKey = NamespacedKey<DawnTerminalCommandInfo>.Vanilla(nodeToCheck.name);
                TerminalCommandBasicInformation basicInformation = new(nodeToCheck.terminalEvent.ToCapitalized() + "Command", "TerminalEvent", $"Runs the following event: {nodeToCheck.terminalEvent}", ClearText.None);
                HashSet<NamespacedKey> tags = [DawnLibTags.IsExternal];
                DawnEventDrivenCommandInfo eventDrivenCommandInfo = new(nodeToCheck, onTerminalEvent);

                DawnTerminalCommandInfo terminalCommandInfo = new(namespacedKey, basicInformation, [terminalKeyword], tags, null, null, null, null, null, eventDrivenCommandInfo, null, null);
                LethalContent.TerminalCommands.Register(terminalCommandInfo);
                nodeToCheck.SetDawnInfo(terminalCommandInfo);
            }
        }

        LethalContent.TerminalCommands.Freeze();
        // Grab the vanilla references here
        orig(self);
    }

    private static void RegisterDawnTerminalCommands(On.Terminal.orig_Awake orig, Terminal self)
    {
        foreach (DawnTerminalCommandInfo terminalCommandInfo in LethalContent.TerminalCommands.Values)
        {
            if (terminalCommandInfo.ShouldSkipIgnoreOverride())
                continue;

            terminalCommandInfo.InjectCommandIntoTerminal(self);
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
            if (commandInfo.SimpleQueryCommandInfo != null && commandInfo.SimpleQueryCommandInfo.ResultNode == node)
            {
                if (!commandInfo.SimpleQueryCommandInfo.ContinueCondition.Invoke())
                {
                    nodeToLoad = commandInfo.SimpleQueryCommandInfo.CancelNode;
                    commandInfo.SimpleQueryCommandInfo.OnContinuedEvent.Invoke(false);
                }
                else
                {
                    commandInfo.SimpleQueryCommandInfo.OnContinuedEvent.Invoke(true);
                }
            }
            else if (commandInfo.ComplexQueryCommandInfo != null)
            {
                for (int i = 0; i < commandInfo.ComplexQueryCommandInfo.ResultNodes.Count; i++)
                {
                    if (commandInfo.ComplexQueryCommandInfo.ResultNodes[i] == node)
                    {
                        if (!commandInfo.ComplexQueryCommandInfo.ContinueConditions[i].Invoke())
                        {
                            nodeToLoad = commandInfo.ComplexQueryCommandInfo.CancelNode;
                            commandInfo.ComplexQueryCommandInfo.OnContinuedEvents[i].Invoke(false);
                        }
                        else
                        {
                            commandInfo.ComplexQueryCommandInfo.OnContinuedEvents[i].Invoke(true);
                        }
                        break;
                    }
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

        if (self.DawnTryResolveKeyword(playerWord, out TerminalKeyword? NonNullResult))
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
        if (self.DawnTryResolveKeyword(playerWord, out TerminalKeyword? NonNullResult))
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
                if (keyword.TryGetKeywordInfoText(out string? result))
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
}