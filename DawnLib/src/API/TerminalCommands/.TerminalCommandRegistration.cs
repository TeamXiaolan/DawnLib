using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dawn.Internal;
using Dawn.Utils;
using MonoMod.RuntimeDetour;

namespace Dawn;

[HarmonyLib.HarmonyPatch]
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

    [HarmonyLib.HarmonyPatch(typeof(Terminal), nameof(Terminal.RunTerminalEvents)), HarmonyLib.HarmonyPrefix, HarmonyLib.HarmonyPriority(int.MaxValue)]
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
            if (terminalKeyword.accessTerminalObjects)
            {
                DawnTerminalObjectCommandInfo terminalObjectCommandInfo = new();
                string formattedName = FormatNodeName(terminalKeyword.name);
                NamespacedKey<DawnTerminalCommandInfo>? terminalObjectNamespacedKey = TerminalCommandKeys.GetByReflection(formattedName);
                if (terminalObjectNamespacedKey == null)
                {
                    terminalObjectNamespacedKey = NamespacedKey<DawnTerminalCommandInfo>.From("unknown_lib", formattedName);
                }

                if (LethalContent.TerminalCommands.ContainsKey(terminalObjectNamespacedKey))
                    continue;

                HashSet<NamespacedKey> terminalCommandTags = [DawnLibTags.IsExternal];

                TerminalCommandBasicInformation terminalCommandBasicInformation = new($"{formattedName.ToCapitalized()}Command", "Vanilla Command", "Terminal Object Command.", ClearText.None);
                DawnTerminalCommandInfo terminalCommandInfo = new(terminalObjectNamespacedKey, terminalCommandBasicInformation, [terminalKeyword], true, terminalCommandTags, null, null, null, null, terminalObjectCommandInfo, null, null, null);
                LethalContent.TerminalCommands.Register(terminalCommandInfo);
                continue;
            }

            if (terminalKeyword.isVerb && terminalKeyword.compatibleNouns.Length > 0)
            {
                List<TerminalNode> complexResultNodes = new();
                List<TerminalKeyword> secondaryKeywords = new();
                foreach (CompatibleNoun compatibleNoun in terminalKeyword.compatibleNouns)
                {
                    if (compatibleNoun.result == null || compatibleNoun.noun == null)
                        continue;

                    complexResultNodes.Add(compatibleNoun.result);
                    secondaryKeywords.Add(compatibleNoun.noun);
                }

                Debuggers.Terminal?.Log($"Getting ComplexCommand with base terminalKeyword {terminalKeyword.name}");
                DawnComplexCommandInfo complexCommandInfo = new(complexResultNodes, secondaryKeywords);
                string formattedName = FormatNodeName(terminalKeyword.name);
                NamespacedKey<DawnTerminalCommandInfo>? complexNamespacedKey = TerminalCommandKeys.GetByReflection(formattedName);
                if (complexNamespacedKey == null)
                {
                    complexNamespacedKey = NamespacedKey<DawnTerminalCommandInfo>.From("unknown_lib", formattedName);
                }

                if (LethalContent.TerminalCommands.ContainsKey(complexNamespacedKey))
                    continue;

                HashSet<NamespacedKey> complexCommandTags = [DawnLibTags.IsExternal];
                ClearText complexClearText = ClearText.None;
                if (complexCommandInfo.ResultNodes.Any(x => x.clearPreviousText))
                {
                    complexClearText |= ClearText.Result;
                }

                TerminalCommandBasicInformation complexCommandBasicInformation = new($"{formattedName.ToCapitalized()}Command", "Vanilla Command", "Complex Command.", complexClearText);
                DawnTerminalCommandInfo complexTerminalCommandInfo = new(complexNamespacedKey, complexCommandBasicInformation, [terminalKeyword], true, complexCommandTags, null, null, complexCommandInfo, null, null, null, null, null);
                LethalContent.TerminalCommands.Register(complexTerminalCommandInfo);

                foreach (TerminalNode complexResultNode in complexResultNodes)
                {
                    complexResultNode.SetDawnInfo(complexTerminalCommandInfo);
                    if (complexResultNode.terminalOptions == null || complexResultNode.terminalOptions.Length != 2 || complexResultNode.name == "FileCabinet1" || complexResultNode.name == "Cupboard1" || complexResultNode.name == "Bunkbeds1")
                        continue;

                    Debuggers.Terminal?.Log($"Getting SimpleQueryCommand from {complexResultNode.name} with base terminalKeyword {terminalKeyword.name}");
                    TerminalNode simpleQueryCommandResultNode = complexResultNode.terminalOptions[0].result;
                    TerminalNode simpleQueryCommandCancelNode = complexResultNode.terminalOptions[1].result;
                    TerminalKeyword simpleQueryCommandConfirmKeyword = complexResultNode.terminalOptions[0].noun;
                    TerminalKeyword simpleQueryCommandCancelKeyword = complexResultNode.terminalOptions[1].noun;

                    if (complexResultNode.terminalOptions[0].noun.word == "deny")
                    {
                        Debuggers.Terminal?.Log($"Switcharoo because deny is suddenly on top instead of bottom!");
                        simpleQueryCommandResultNode = complexResultNode.terminalOptions[1].result;
                        simpleQueryCommandCancelNode = complexResultNode.terminalOptions[0].result;
                        simpleQueryCommandConfirmKeyword = complexResultNode.terminalOptions[1].noun;
                        simpleQueryCommandCancelKeyword = complexResultNode.terminalOptions[0].noun;
                    }

                    formattedName = FormatNodeName(simpleQueryCommandResultNode.name);
                    NamespacedKey<DawnTerminalCommandInfo>? simpleQueryNamespacedKey = TerminalCommandKeys.GetByReflection(formattedName);
                    if (simpleQueryNamespacedKey == null)
                    {
                        simpleQueryNamespacedKey = NamespacedKey<DawnTerminalCommandInfo>.From("unknown_lib", formattedName);
                    }

                    if (LethalContent.TerminalCommands.ContainsKey(simpleQueryNamespacedKey))
                        continue;

                    DawnSimpleQueryCommandInfo simpleQueryCommandInfo = new(simpleQueryCommandResultNode, complexResultNode, complexResultNode.terminalOptions[1].result, complexResultNode.terminalOptions[0].noun, complexResultNode.terminalOptions[1].noun, () => true, _ => { });
                    HashSet<NamespacedKey> simpleQueryTags = [DawnLibTags.IsExternal];
                    ClearText simpleQueryClearText = ClearText.None;
                    if (simpleQueryCommandInfo.ResultNode.clearPreviousText)
                    {
                        simpleQueryClearText |= ClearText.Result;
                    }

                    if (simpleQueryCommandInfo.ContinueOrCancelNode.clearPreviousText)
                    {
                        simpleQueryClearText |= ClearText.Query;
                    }

                    if (simpleQueryCommandInfo.CancelNode.clearPreviousText)
                    {
                        simpleQueryClearText |= ClearText.Cancel;
                    }

                    TerminalCommandBasicInformation simpleQueryBasicInformation = new($"{formattedName.ToCapitalized()}Command", "Vanilla Command", "Complex Command.", simpleQueryClearText);
                    DawnTerminalCommandInfo simpleQueryTerminalCommandInfo = new(simpleQueryNamespacedKey, simpleQueryBasicInformation, [terminalKeyword], true, simpleQueryTags, null, simpleQueryCommandInfo, null, null, null, null, null, null);
                    LethalContent.TerminalCommands.Register(simpleQueryTerminalCommandInfo);
                    simpleQueryCommandResultNode.SetDawnInfo(simpleQueryTerminalCommandInfo);
                }
            }

            List<TerminalNode> terminalNodesToCheck = GetAllNodesRelatedToAKeyword(terminalKeyword);
            foreach (TerminalNode nodeToCheck in terminalNodesToCheck)
            {
                if (TryGetEventCommandFromNode(nodeToCheck, out DawnEventDrivenCommandInfo? eventDrivenCommandInfo))
                {
                    Debuggers.Terminal?.Log($"Getting EventDrivenCommand from {nodeToCheck.name} with base terminalKeyword {terminalKeyword.name}");
                    string formattedName = FormatNodeName(nodeToCheck.name);
                    NamespacedKey<DawnTerminalCommandInfo>? eventDrivenNamespacedKey = TerminalCommandKeys.GetByReflection(formattedName);
                    if (eventDrivenNamespacedKey == null)
                    {
                        eventDrivenNamespacedKey = NamespacedKey<DawnTerminalCommandInfo>.From("unknown_lib", formattedName);
                    }

                    if (LethalContent.TerminalCommands.ContainsKey(eventDrivenNamespacedKey))
                        continue;

                    HashSet<NamespacedKey> eventDrivenTags = [DawnLibTags.IsExternal];

                    TerminalCommandBasicInformation eventDrivenBasicInformation = new($"{formattedName.ToCapitalized()}Command", "Vanilla Command", "Event Driven Command.", eventDrivenCommandInfo.ResultNode.clearPreviousText ? ClearText.Result : ClearText.None);
                    DawnTerminalCommandInfo eventDrivenTerminalCommandInfo = new(eventDrivenNamespacedKey, eventDrivenBasicInformation, [terminalKeyword], true, eventDrivenTags, null, null, null, null, null, eventDrivenCommandInfo, null, null);
                    LethalContent.TerminalCommands.Register(eventDrivenTerminalCommandInfo);
                    nodeToCheck.SetDawnInfo(eventDrivenTerminalCommandInfo);
                }
            }
        }

        LethalContent.TerminalCommands.Freeze();
        // Grab the vanilla references here
        orig(self);
    }

    private static string FormatNodeName(string terminalNodeName)
    {
        string name = terminalNodeName;
        name = ReplaceInternalLevelNames(name);
        name = NamespacedKey.NormalizeStringForNamespacedKey(name, true);
        return name;
    }

    private static readonly Dictionary<string, string> _internalToHumanRouteNames = new()
    {
        { "5route", "EmbrionRoute" },
        { "7route", "DineRoute" },
        { "8route", "TitanRoute" },
        { "20route", "AdamanceRoute" },
        { "21route", "OffenseRoute" },
        { "41route", "ExperimentationRoute" },
        { "56route", "VowRoute" },
        { "61route", "MarchRoute" },
        { "68route", "ArtificeRoute" },
        { "85route", "RendRoute" },
        { "220route", "AssuranceRoute" },
    };

    private static string ReplaceInternalLevelNames(string input)
    {
        foreach ((string internalName, string humanName) in _internalToHumanRouteNames)
        {
            input = input.Replace(internalName, humanName);
        }
        return input;
    }

    private static bool TryGetEventCommandFromNode(TerminalNode node, [NotNullWhen(true)] out DawnEventDrivenCommandInfo? eventDrivenCommandInfo)
    {
        eventDrivenCommandInfo = null;
        if (!string.IsNullOrEmpty(node.terminalEvent))
        {
            Action<Terminal, TerminalNode>? onTerminalEvent = null;
            switch (node.terminalEvent)
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
                return false;

            node.terminalEvent = string.Empty;
            eventDrivenCommandInfo = new DawnEventDrivenCommandInfo(node, onTerminalEvent);
            return true;
        }
        return false;
    }

    private static List<TerminalNode> GetAllNodesRelatedToAKeyword(TerminalKeyword keyword)
    {
        List<TerminalNode> foundNodes = new();
        if (keyword.specialKeywordResult != null)
        {
            foundNodes.Add(keyword.specialKeywordResult);
            if (keyword.specialKeywordResult.terminalOptions != null)
            {
                foreach (CompatibleNoun compatibleNoun in keyword.specialKeywordResult.terminalOptions)
                {
                    if (compatibleNoun.result == null)
                        continue;

                    foundNodes.Add(compatibleNoun.result);
                }
            }
        }
        return foundNodes;
    }

    private static void RegisterDawnTerminalCommands(On.Terminal.orig_Awake orig, Terminal self)
    {
        foreach (DawnTerminalCommandInfo terminalCommandInfo in LethalContent.TerminalCommands.Values)
        {
            if (terminalCommandInfo.ShouldSkipIgnoreOverride())
                continue;

            if (!terminalCommandInfo.BuildOnTerminalAwake)
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
                            if (commandInfo.ComplexQueryCommandInfo.CancelNode != null)
                            {
                                nodeToLoad = commandInfo.ComplexQueryCommandInfo.CancelNode;
                            }
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