using System;
using System.Collections.Generic;
using UnityEngine.Events;
using static Dawn.TerminalCommandRegistration;

namespace Dawn;
public class TerminalCommandInfoBuilder : BaseInfoBuilder<DawnTerminalCommandInfo, TerminalNode, TerminalCommandInfoBuilder>
{
    private DawnQueryCommandInfo? _queryCommandInfo = null;

    public class QueryCommandBuilder
    {
        private TerminalCommandInfoBuilder _parentBuilder;

        private Func<string> _queryFunc, _cancelFunc;
        private string _continueKeyword, _cancelKeyword;
        private FuncProvider<bool> _continueProvider;
        private DawnEvent<bool>? _onQueryContinuedEvent;

        internal QueryCommandBuilder(TerminalCommandInfoBuilder parent)
        {
            _parentBuilder = parent;
        }

        public QueryCommandBuilder SetQuery(Func<string> queryFunc)
        {
            _queryFunc = queryFunc;
            return this;
        }

        public QueryCommandBuilder SetCancel(Func<string> cancelFunc)
        {
            _cancelFunc = cancelFunc;
            return this;
        }

        public QueryCommandBuilder SetContinueConditions(FuncProvider<bool> continueProvider)
        {
            _continueProvider = continueProvider;
            return this;
        }

        public QueryCommandBuilder SetQueryEvent(DawnEvent<bool> onQueryContinuedEvent)
        {
            _onQueryContinuedEvent = onQueryContinuedEvent;
            return this;
        }

        public QueryCommandBuilder SetContinueWord(string continueKeyword)
        {
            _continueKeyword = continueKeyword;
            return this;
        }

        public QueryCommandBuilder SetCancelWord(string cancelKeyword)
        {
            _cancelKeyword = cancelKeyword;
            return this;
        }

        internal DawnQueryCommandInfo Build()
        {
            if (_continueProvider == null)
            {
                _continueProvider = new FuncProvider<bool>(() => true);
            }
            return new DawnQueryCommandInfo(_queryFunc, _cancelFunc, _continueKeyword, _cancelKeyword, _continueProvider, _onQueryContinuedEvent);
        }
    }
    // Allow setting a query with a continue and cancel word, query text

    public TerminalCommandInfoBuilder DefineQueryCommand(Action<QueryCommandBuilder> callback)
    {
        QueryCommandBuilder builder = new(this);
        callback(builder);
        _queryCommandInfo = builder.Build();
        return this;
    }

    private IProvider<bool> _isEnabled;
    private IProvider<List<string>> _validKeywords;
    private Func<string> _mainText;
    private UnityEvent? _customUnityBuildEvent = null;
    private UnityEvent? _customUnityDestroyEvent = null;
    private DawnEvent? _customDawnBuildEvent = null;
    private DawnEvent? _customDawnDestroyEvent = null;
    private string _commandName, _categoryName, _description;
    private bool _acceptInput;
    private ClearText _clearTextFlags;
    private bool _overrideKeywords = false;
    private ITerminalKeyword.DawnKeywordType _overridePriority = ITerminalKeyword.DawnKeywordType.DawnCommand;

    public TerminalCommandInfoBuilder(NamespacedKey<DawnTerminalCommandInfo> key, string commandName, TerminalNode value) : base(key, value)
    {
        _commandName = commandName;
    }

    public TerminalCommandInfoBuilder SetEnabled(IProvider<bool> isEnabled)
    {
        _isEnabled = isEnabled;
        return this;
    }

    public TerminalCommandInfoBuilder SetCustomBuildEvent(UnityEvent customBuildEvent)
    {
        _customUnityBuildEvent = customBuildEvent;
        return this;
    }

    public TerminalCommandInfoBuilder SetCustomBuildEvent(DawnEvent dawnBuildEvent)
    {
        _customDawnBuildEvent = dawnBuildEvent;
        return this;
    }

    public TerminalCommandInfoBuilder SetCustomDestroyEvent(UnityEvent customDestroyEvent)
    {
        _customUnityDestroyEvent = customDestroyEvent;
        return this;
    }

    public TerminalCommandInfoBuilder SetCustomDestroyEvent(DawnEvent dawnDestroyEvent)
    {
        _customDawnDestroyEvent = dawnDestroyEvent;
        return this;
    }

    public TerminalCommandInfoBuilder SetMainText(Func<string> mainText)
    {
        _mainText = mainText;
        return this;
    }

    public TerminalCommandInfoBuilder SetKeywords(IProvider<List<string>> validKeywords)
    {
        _validKeywords = validKeywords;
        return this;
    }

    public TerminalCommandInfoBuilder SetClearTextFlags(ClearText clearTextFlags)
    {
        value.clearPreviousText = clearTextFlags.HasFlag(ClearText.Result);
        _clearTextFlags = clearTextFlags;
        return this;
    }

    public TerminalCommandInfoBuilder SetCategoryName(string categoryName)
    {
        _categoryName = categoryName;
        return this;
    }

    public TerminalCommandInfoBuilder SetDescription(string description)
    {
        _description = description;
        return this;
    }

    public TerminalCommandInfoBuilder SetAcceptInput(bool acceptInput)
    {
        _acceptInput = acceptInput;
        return this;
    }

    // <summary>Override any existing keywords and manually set the keyword's priority</summary>
    /* <remarks>
        WARNING: Setting this to true can cause compatibility issues with other mods! Use with Caution!!
        You do not need to set this to false if you have not changed the default value
        Overriding a vanilla keyword will permanently alter the keyword result. Vanilla does not rebuild keywords automatically on lobby reload.
      </remarks> */

    public TerminalCommandInfoBuilder SetOverrideKeywords(bool value, ITerminalKeyword.DawnKeywordType overridePriority = ITerminalKeyword.DawnKeywordType.DawnCommand)
    {
        _overrideKeywords = value;
        _overridePriority = overridePriority;
        return this;
    }

    override internal DawnTerminalCommandInfo Build()
    {
        TerminalCommandRegistrationBuilder commandRegistrationBuilder = new TerminalCommandRegistrationBuilder(_commandName, value, _mainText);
        commandRegistrationBuilder.SetCategory(_categoryName);
        commandRegistrationBuilder.SetDescription(_description);
        commandRegistrationBuilder.SetEnabled(_isEnabled);
        if (_validKeywords.Provide().Count <= 0)
        {
            DawnPlugin.Logger.LogError($"No valid keywords provided for command: {_commandName}");
        }
        commandRegistrationBuilder.SetKeywords(_validKeywords);
        commandRegistrationBuilder.SetClearText(_clearTextFlags);
        commandRegistrationBuilder.SetAcceptInput(_acceptInput);
        commandRegistrationBuilder.SetOverrideExistingKeywords(_overrideKeywords, _overridePriority);

        if (_queryCommandInfo != null)
        {
            commandRegistrationBuilder.SetupQuery(_queryCommandInfo.QueryFunc);
            commandRegistrationBuilder.SetupCancel(_queryCommandInfo.CancelFunc);
            commandRegistrationBuilder.SetCancelWord(_queryCommandInfo.CancelKeyword);
            commandRegistrationBuilder.SetContinueWord(_queryCommandInfo.ContinueKeyword);
        }

        if (_customDawnBuildEvent == null && _customUnityBuildEvent == null)
        {
            commandRegistrationBuilder.BuildOnTerminalAwake();
        }
        else
        {
            SetCustomBuildEvents(commandRegistrationBuilder);
        }

        SetCustomDestroyEvents(commandRegistrationBuilder);

        DawnTerminalCommandInfo info = new(key, tags, value, _queryCommandInfo, customData);
        return info;
    }

    //Sets any custom build event types if they are not null
    private void SetCustomBuildEvents(TerminalCommandRegistrationBuilder commandRegistrationBuilder)
    {
        if (_customUnityBuildEvent != null)
        {
            commandRegistrationBuilder.SetCustomBuildEvent(_customUnityBuildEvent);
        }

        if (_customDawnBuildEvent != null)
        {
            commandRegistrationBuilder.SetCustomBuildEvent(_customDawnBuildEvent);
        }
    }

    //Sets any custom destroy event types if they are not null
    private void SetCustomDestroyEvents(TerminalCommandRegistrationBuilder commandRegistrationBuilder)
    {
        if (_customUnityDestroyEvent != null)
        {
            commandRegistrationBuilder.SetCustomDestroyEvent(_customUnityDestroyEvent);
        }

        if (_customDawnDestroyEvent != null)
        {
            commandRegistrationBuilder.SetCustomDestroyEvent(_customDawnDestroyEvent);
        }
    }
}