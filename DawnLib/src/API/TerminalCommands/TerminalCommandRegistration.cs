using System;
using System.Collections.Generic;
using Dawn.Internal;
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
    public UnityEvent? DestroyEvent;

    //Query-Style
    public Func<string>? QueryFunction;
    public Func<string>? CancelFunction;
    public string? ContinueWord;
    public string? CancelWord;

    //for commands that accept input after the keyword (ie. "fov 90" where the command is <fov> and <90> is the additional input)
    //will apply to keyword via interface
    public bool AcceptAdditionalText = false;

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
        TerminalPatches.OnTerminalAwake += Build;
        return this;
    }


    //NOTE: The event in this param must invoke AFTER Terminal Awake in order to work
    public TerminalCommandRegistrationBuilder SetCustomBuildEvent(UnityEvent buildEvent)
    {
        buildEvent.AddListener(Build);
        return this;
    }

    public TerminalCommandRegistrationBuilder SetCustomDestroyEvent(UnityEvent destroyEvent)
    {
        register.DestroyEvent = destroyEvent;
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

        List<TerminalKeyword> keywords = [];
        List<string> words = register.KeywordList.Provide();

        foreach (string word in words)
        {
            Debuggers.Terminal?.Log($"Creating keyword [ {word} ] for command [ {register.Name} ]");
            TerminalKeyword addKeyword = new TerminalKeywordBuilder($"{register.Name}_{word}", word, ITerminalKeyword.DawnKeywordType.DawnCommand)
                .SetAcceptInput(register.AcceptAdditionalText)
                .Build();

            keywords.Add(addKeyword);
        }

        TerminalCommandBuilder commandbuilder = new(register.Name);
        if(register.DestroyEvent != null)
        {
            //removes terminaldisable destroy event for specified event
            commandbuilder.SetCustomDestroyEvent(register.DestroyEvent);
        }
        commandbuilder.SetResultNode(resultNode);
        commandbuilder.AddResultAction(register.ResultFunction);
        commandbuilder.AddKeyword(keywords);

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
