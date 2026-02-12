using System;
using System.Collections.Generic;

namespace Dawn;

[Flags]
public enum ClearText
{
    None = 0,
    Continue = 1 << 0,
    Query = 1 << 1,
    Cancel = 1 << 2
}

public class TerminalCommandInfoBuilder : BaseInfoBuilder<DawnTerminalCommandInfo, TerminalNode, TerminalCommandInfoBuilder>
{
    private DawnQueryCommandInfo? _queryCommandInfo = null;

    public class QueryCommandBuilder
    {
        private TerminalCommandInfoBuilder _parentBuilder;

        private Func<string> _continueFunc, _cancelFunc;
        private string _continueString, _cancelString;
        private Func<bool> _continueCondition;
        private Action<bool>? _onQueryContinuedEvent;

        internal QueryCommandBuilder(TerminalCommandInfoBuilder parent)
        {
            _parentBuilder = parent;
        }

        public QueryCommandBuilder SetContinue(Func<string> queryFunc)
        {
            _continueFunc = queryFunc;
            return this;
        }

        public QueryCommandBuilder SetCancel(Func<string> cancelFunc)
        {
            _cancelFunc = cancelFunc;
            return this;
        }

        public QueryCommandBuilder SetContinueConditions(Func<bool> continueProvider)
        {
            _continueCondition = continueProvider;
            return this;
        }

        public QueryCommandBuilder SetQueryEvent(Action<bool> onQueryContinuedEvent)
        {
            _onQueryContinuedEvent = onQueryContinuedEvent;
            return this;
        }

        public QueryCommandBuilder SetContinueWord(string continueKeyword)
        {
            _continueString = continueKeyword;
            return this;
        }

        public QueryCommandBuilder SetCancelWord(string cancelKeyword)
        {
            _cancelString = cancelKeyword;
            return this;
        }

        internal DawnQueryCommandInfo Build()
        {
            if (_continueCondition == null)
            {
                _continueCondition = new Func<bool>(() => true);
            }

            TerminalNode _continueNode = new TerminalNodeBuilder($"{_parentBuilder.key}:ContinueNode")
                                            .SetDisplayText(_continueFunc.Invoke())
                                            .SetClearPreviousText(true)
                                            .SetMaxCharactersToType(35)
                                            .Build();
            
            TerminalNode _cancelNode = new TerminalNodeBuilder($"{_parentBuilder.key}:CancelNode")
                                            .SetDisplayText(_cancelFunc.Invoke())
                                            .SetClearPreviousText(true)
                                            .SetMaxCharactersToType(35)
                                            .Build();

            if (string.IsNullOrEmpty(_cancelString))
            {
                _cancelString = "deny";
                DawnPlugin.Logger.LogWarning($"Query '{_parentBuilder.key}' didn't set cancel word. Defaulting to '{_cancelString}'");
            }

            if (string.IsNullOrEmpty(_continueString))
            {
                _continueString = "confirm";
                DawnPlugin.Logger.LogWarning($"Query '{_parentBuilder.key}' didn't set continue word. Defaulting to '{_continueString}'");
            }

            TerminalKeyword _continueKeyword = new TerminalKeywordBuilder($"{_parentBuilder.key}:ContinueKeyword", _continueString, ITerminalKeyword.DawnKeywordType.DawnCommand)
                                                .Build();
            
            TerminalKeyword _cancelKeyword = new TerminalKeywordBuilder($"{_parentBuilder.key}:CancelKeyword", _cancelString, ITerminalKeyword.DawnKeywordType.DawnCommand)
                                                .Build();

            _continueNode.clearPreviousText = _parentBuilder._clearTextFlags.HasFlag(ClearText.Continue);
            _cancelNode.clearPreviousText = _parentBuilder._clearTextFlags.HasFlag(ClearText.Cancel);

            return new DawnQueryCommandInfo(_continueNode, _cancelNode, _continueFunc, _cancelFunc, _continueKeyword, _cancelKeyword, _continueCondition, _onQueryContinuedEvent);
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
    private string _commandName, _categoryName, _description;
    private bool _acceptInput;
    private ClearText _clearTextFlags;
    private bool _overrideKeywords = false;
    private ITerminalKeyword.DawnKeywordType _overridePriority = ITerminalKeyword.DawnKeywordType.DawnCommand;

    public TerminalCommandInfoBuilder(NamespacedKey<DawnTerminalCommandInfo> key, TerminalNode value) : base(key, value)
    {
    }

    public TerminalCommandInfoBuilder SetEnabled(IProvider<bool> isEnabled)
    {
        _isEnabled = isEnabled;
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
        return new DawnTerminalCommandInfo(key, value, tags, _queryCommandInfo, customData);
    }
}