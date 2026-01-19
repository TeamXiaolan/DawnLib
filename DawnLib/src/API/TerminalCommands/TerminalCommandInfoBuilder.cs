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
            return new DawnQueryCommandInfo(_queryFunc, _cancelFunc, _continueKeyword, _cancelKeyword);
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
    private UnityEvent? _customBuildEvent = null;
    private string _commandName, _categoryName, _description;
    private bool _acceptInput;
    private ClearText _clearTextFlags;

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
        _customBuildEvent = customBuildEvent;
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

        if (_queryCommandInfo != null)
        {
            commandRegistrationBuilder.SetupQuery(_queryCommandInfo.QueryFunc);
            commandRegistrationBuilder.SetupCancel(_queryCommandInfo.CancelFunc);
            commandRegistrationBuilder.SetCancelWord(_queryCommandInfo.CancelKeyword);
            commandRegistrationBuilder.SetContinueWord(_queryCommandInfo.ContinueKeyword);
        }

        if (_customBuildEvent != null)
        {
            commandRegistrationBuilder.SetCustomBuildEvent(_customBuildEvent);
        }
        else
        {
            commandRegistrationBuilder.BuildOnTerminalAwake();
        }
        DawnTerminalCommandInfo info = new DawnTerminalCommandInfo(key, tags, value, _queryCommandInfo, customData);
        return info;
    }
}