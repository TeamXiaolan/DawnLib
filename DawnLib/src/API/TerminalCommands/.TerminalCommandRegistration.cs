using System;
using System.Collections.Generic;
using Dawn.Internal;
using Dawn.Utils;
using UnityEngine.Events;
using static Dawn.TerminalCommandRegistration;

namespace Dawn;

//for use with creating terminal commands from plugin awake
public class TerminalCommandRegistration
{
    //--- Required Values
    public string Name = string.Empty;
    public IProvider<bool> IsEnabled = null!;
    public ClearText ClearTextOn = ClearText.Result;
    public IProvider<List<string>> KeywordList = null!;
    public Func<string> ResultFunction = null!;

    //--- Optional Values
    public string? Category;
    public string? Description;
    public UnityEvent? UnityDestroyEvent;
    public DawnEvent? DawnDestroyEvent;

    //Query-Style
    public Func<string>? QueryFunction;
    public Func<string>? CancelFunction;
    public string? ContinueWord;
    public string? CancelWord;

    //for commands that accept input after the keyword (ie. "fov 90" where the command is <fov> and <90> is the additional input)
    //will apply to keyword via interface
    public bool AcceptAdditionalText = false;

    //allow user to ignore checking for existing keywords and overwrite them at build
    public bool OverrideExistingKeywords = false;
    public ITerminalKeyword.DawnKeywordType OverridePriority = ITerminalKeyword.DawnKeywordType.DawnCommand;

    internal TerminalCommandRegistration(string commandName)
    {
        Name = commandName;
    }

    //Which nodes should clear text on load,
    [Flags]
    public enum ClearText
    {
        None = 0,
        Result = 1 << 0,
        Query = 1 << 1,
        Cancel = 1 << 2
    }

    internal static DawnEvent OnTerminalAwake = new();
    internal static DawnEvent OnTerminalDisable = new();

    internal static void Init()
    {
        On.Terminal.Awake += TerminalAwakeHook;
        On.Terminal.OnDisable += TerminalDisableHook;

        On.Terminal.LoadNewNode += HandleQueryEventAndContinueCondition;
        On.Terminal.Start += AssignTerminalPriorites;
        On.Terminal.CheckForExactSentences += CheckForExactSentencesPrefix;
        On.Terminal.ParseWord += ParseWordPrefix;
        On.Terminal.ParsePlayerSentence += HandleDawnCommand;
    }

    private static void HandleQueryEventAndContinueCondition(On.Terminal.orig_LoadNewNode orig, Terminal self, TerminalNode node)
    {
        TerminalNode nodeToLoad = node;

        if (nodeToLoad.HasDawnInfo())
        {
            DawnTerminalCommandInfo commandInfo = nodeToLoad.GetDawnInfo();
            if (commandInfo.ResultNode == node && commandInfo.QueryCommandInfo != null)
            {
                if (!commandInfo.QueryCommandInfo.ContinueProvider.Provide())
                {
                    nodeToLoad = commandInfo.QueryCommandInfo.CancelNode!;
                    if (nodeToLoad.HasCommandFunction())
                    {
                        nodeToLoad.displayText = nodeToLoad.GetCommandFunction().Invoke();
                    }
                }
                else
                {
                    commandInfo.QueryCommandInfo.OnQueryContinuedEvent?.Invoke();
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

        if (self.DawnTryResolveKeyword(playerWord, out TerminalKeyword NonNullResult))
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
        if (self.DawnTryResolveKeyword(playerWord, out TerminalKeyword NonNullResult))
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

    private static void TerminalDisableHook(On.Terminal.orig_OnDisable orig, Terminal self)
    {
        //All commands use this event to destroy themselves between lobby loads by default
        OnTerminalDisable.Invoke();

        //still need to run the method
        orig(self);
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
                if (keyword.TryGetKeywordInfoText(out string result))
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

    private static void TerminalAwakeHook(On.Terminal.orig_Awake orig, Terminal self)
    {
        orig(self);
        //below will have many terminal commands begin building on the below invoke
        //only commands with a custom defined build event will not use this event
        OnTerminalAwake.Invoke();
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

public class TerminalCommandRegistrationBuilder(string CommandName, TerminalNode resultNode, Func<string> mainFunction)
{
    private readonly TerminalCommandRegistration register = new(CommandName)
    {
        ResultFunction = mainFunction
    };


    public TerminalCommandRegistrationBuilder SetKeywords(IProvider<List<string>> keywords)
    {
        register.KeywordList = keywords;
        return this;
    }

    // WARNING: Setting this to true can cause compatibility issues with other mods! Use with Caution!!
    // You do not need to set this to false if you have not changed the default value
    // Overriding a vanilla keyword will permanently alter the keyword result. Vanilla does not rebuild keywords automatically on lobby reload
    public TerminalCommandRegistrationBuilder SetOverrideExistingKeywords(bool value, ITerminalKeyword.DawnKeywordType KeywordPriority = ITerminalKeyword.DawnKeywordType.DawnCommand)
    {
        register.OverrideExistingKeywords = value;
        register.OverridePriority = KeywordPriority;
        return this;
    }

    public TerminalCommandRegistrationBuilder SetupQuery(Func<string> queryFunc)
    {
        register.QueryFunction = queryFunc;
        return this;
    }

    public TerminalCommandRegistrationBuilder SetupCancel(Func<string> cancelFunc)
    {
        register.CancelFunction = cancelFunc;
        return this;
    }

    public TerminalCommandRegistrationBuilder SetCancelWord(string value)
    {
        register.CancelWord = value;
        return this;
    }

    public TerminalCommandRegistrationBuilder SetContinueWord(string value)
    {
        register.ContinueWord = value;
        return this;
    }

    public TerminalCommandRegistrationBuilder BuildOnTerminalAwake()
    {
        OnTerminalAwake.OnInvoke += Build;
        return this;
    }

    // <summary>Override standard build event (TerminalAwake) for a custom UnityEvent to invoke TerminalCommand Build</summary>
    // <remarks>NOTE: The event in this param must invoke AFTER Terminal Awake in order to work</remarks>
    public TerminalCommandRegistrationBuilder SetCustomBuildEvent(UnityEvent unityBuildEvent)
    {
        unityBuildEvent?.AddListener(Build);
        return this;
    }

    // <summary>Override standard build event (TerminalAwake) for a custom DawnEvent to invoke TerminalCommand Build</summary>
    // <remarks>NOTE: The event in this param must invoke AFTER Terminal Awake in order to work</remarks>
    public TerminalCommandRegistrationBuilder SetCustomBuildEvent(DawnEvent dawnBuildEvent)
    {
        if (dawnBuildEvent != null)
        {
            dawnBuildEvent.OnInvoke += Build;
        }
            
        return this;
    }

    // <summary>Override standard destroy event (TerminalDisable) for a custom UnityEvent to invoke TerminalCommand Destroy</summary>
    // <remarks>NOTE: This event will not be listened to until AFTER the command has been built</remarks>
    public TerminalCommandRegistrationBuilder SetCustomDestroyEvent(UnityEvent unityDestroyEvent)
    {
        register.UnityDestroyEvent = unityDestroyEvent;
        return this;
    }

    // <summary>Override standard destroy event (TerminalDisable) for a custom DawnEvent to invoke TerminalCommand Destroy</summary>
    // <remarks>NOTE: This event will not be listened to until AFTER the command has been built</remarks>
    public TerminalCommandRegistrationBuilder SetCustomDestroyEvent(DawnEvent dawnDestroyEvent)
    {
        register.DawnDestroyEvent = dawnDestroyEvent;
        return this;
    }

    public TerminalCommandRegistrationBuilder SetDescription(string description)
    {
        register.Description = description;
        return this;
    }

    public TerminalCommandRegistrationBuilder SetCategory(string category)
    {
        register.Category = category;
        return this;
    }

    public TerminalCommandRegistrationBuilder SetClearText(ClearText value)
    {
        register.ClearTextOn = value;
        return this;
    }

    public TerminalCommandRegistrationBuilder SetEnabled(IProvider<bool> value)
    {
        register.IsEnabled = value;
        return this;
    }

    public TerminalCommandRegistrationBuilder SetAcceptInput(bool value)
    {
        register.AcceptAdditionalText = value;
        return this;
    }

    private void Build()
    {
        Debuggers.Terminal?.Log($"Attempting to build command [{register.Name}]");

        if (!ShouldBuild())
        {
            DawnPlugin.Logger.LogWarning($"Unable to build command [{register.Name}] due to missing required components!");
            return;
        }

        List<TerminalKeyword> terminalKeywords = [];
        List<string> keywords = register.KeywordList.Provide();

        foreach (string keyword in keywords)
        {
            Debuggers.Terminal?.Log($"Creating keyword [ {keyword} ] for command [ {register.Name} ]");

            if (register.OverrideExistingKeywords)
            {
                TerminalKeyword overrideKeyword = new TerminalKeywordBuilder($"{register.Name}_{keyword}", keyword)
                    .SetAcceptInput(register.AcceptAdditionalText)
                    .Build();

                overrideKeyword.SetKeywordPriority(register.OverridePriority);
                terminalKeywords.Add(overrideKeyword);
            }
            else
            {
                TerminalKeyword addKeyword = new TerminalKeywordBuilder($"{register.Name}_{keyword}", keyword, ITerminalKeyword.DawnKeywordType.DawnCommand)
                    .SetAcceptInput(register.AcceptAdditionalText)
                    .Build();

                terminalKeywords.Add(addKeyword);
            }
        }

        TerminalCommandBuilder commandbuilder = new(register.Name);

        commandbuilder.TrySetDestroyEvents(register);
        commandbuilder.SetResultNode(resultNode);
        commandbuilder.AddResultAction(register.ResultFunction);
        commandbuilder.AddKeyword(terminalKeywords);

        if (IsQueryCommand())
        {
            TerminalNode queryNode = new TerminalNodeBuilder($"{register.Name}_Query")
                .SetDisplayText($"{register.Name} Query")
                .SetClearPreviousText(register.ClearTextOn.HasFlag(ClearText.Query))
                .Build();

            commandbuilder.SetQueryNode(queryNode);

            TerminalNode cancelNode = new TerminalNodeBuilder($"{register.Name}_Cancel")
                .SetDisplayText($"{register.Name} Cancel")
                .SetClearPreviousText(register.ClearTextOn.HasFlag(ClearText.Cancel))
                .Build();

            commandbuilder.SetCancelNode(cancelNode);

            commandbuilder.SetCancelWord(register.CancelWord!);
            commandbuilder.SetContinueWord(register.ContinueWord!);
            commandbuilder.AddCancelAction(register.CancelFunction!);
            commandbuilder.AddQueryAction(register.QueryFunction!);
        }

        commandbuilder.FinishBuild();

    }

    private bool ShouldBuild()
    {
        if (!register.IsEnabled.Provide())
        {
            return false;
        }

        if (register.KeywordList.Provide().Count == 0)
        {
            return false;
        }

        return true;
    }

    private bool IsQueryCommand()
    {
        return register.QueryFunction != null && register.CancelFunction != null && !string.IsNullOrWhiteSpace(register.ContinueWord) && !string.IsNullOrWhiteSpace(register.CancelWord);
    }
}
