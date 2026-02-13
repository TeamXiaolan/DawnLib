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
    private DawnComplexQueryCommandInfo? _complexQueryCommandInfo = null;
    private DawnSimpleQueryCommandInfo? _simpleQueryCommandInfo = null;
    private DawnSimpleCommandInfo? _simpleCommandInfo = null;
    private DawnTerminalObjectCommandInfo? _terminalObjectCommandInfo = null;
    private DawnEventDrivenCommandInfo? _eventDrivenCommandInfo = null;
    private DawnInputCommandInfo? _inputCommandInfo = null;

    public class ComplexQueryCommandBuilder
    {
        private TerminalCommandInfoBuilder _parentBuilder;

        internal ComplexQueryCommandBuilder(TerminalCommandInfoBuilder parent)
        {
            _parentBuilder = parent;
        }

        internal DawnComplexQueryCommandInfo Build()
        {
            return new DawnComplexQueryCommandInfo();
        }
    }

    public class SimpleQueryCommandBuilder
    {
        private TerminalCommandInfoBuilder _parentBuilder;

        private string _continueDisplayText, _cancelDisplayText;
        private string _continueKeyword, _cancelKeyword;
        private Func<bool> _continueCondition;
        private Action<bool>? _onQueryContinuedEvent;

        internal SimpleQueryCommandBuilder(TerminalCommandInfoBuilder parent)
        {
            _parentBuilder = parent;
        }

        public SimpleQueryCommandBuilder SetContinue(string continueDisplayText)
        {
            _continueDisplayText = continueDisplayText;
            return this;
        }

        public SimpleQueryCommandBuilder SetCancel(string cancelDisplayText)
        {
            _cancelDisplayText = cancelDisplayText;
            return this;
        }

        public SimpleQueryCommandBuilder SetContinueConditions(Func<bool> continueProvider)
        {
            _continueCondition = continueProvider;
            return this;
        }

        public SimpleQueryCommandBuilder SetQueryEvent(Action<bool> onQueryContinuedEvent)
        {
            _onQueryContinuedEvent = onQueryContinuedEvent;
            return this;
        }

        public SimpleQueryCommandBuilder SetContinueWord(string continueKeyword)
        {
            _continueKeyword = continueKeyword;
            return this;
        }

        public SimpleQueryCommandBuilder SetCancelWord(string cancelKeyword)
        {
            _cancelKeyword = cancelKeyword;
            return this;
        }

        internal DawnSimpleQueryCommandInfo Build()
        {
            if (_continueCondition == null)
            {
                _continueCondition = new Func<bool>(() => true);
            }

            TerminalNode _continueNode = new TerminalNodeBuilder($"{_parentBuilder.key}:ContinueNode")
                                            .SetDisplayText(_continueDisplayText)
                                            .SetClearPreviousText(true)
                                            .SetMaxCharactersToType(35)
                                            .Build();
            
            TerminalNode _cancelNode = new TerminalNodeBuilder($"{_parentBuilder.key}:CancelNode")
                                            .SetDisplayText(_cancelDisplayText)
                                            .SetClearPreviousText(true)
                                            .SetMaxCharactersToType(35)
                                            .Build();

            if (string.IsNullOrEmpty(_cancelKeyword))
            {
                _cancelKeyword = "deny";
                DawnPlugin.Logger.LogWarning($"Query '{_parentBuilder.key}' didn't set cancel word. Defaulting to '{_cancelKeyword}'");
            }

            if (string.IsNullOrEmpty(_continueKeyword))
            {
                _continueKeyword = "confirm";
                DawnPlugin.Logger.LogWarning($"Query '{_parentBuilder.key}' didn't set continue word. Defaulting to '{_continueKeyword}'");
            }

            TerminalKeyword _continueTerminalKeyword = new TerminalKeywordBuilder($"{_parentBuilder.key}:ContinueKeyword", _continueKeyword, ITerminalKeyword.DawnKeywordType.DawnCommand)
                                                .Build();
            
            TerminalKeyword _cancelTerminalKeyword = new TerminalKeywordBuilder($"{_parentBuilder.key}:CancelKeyword", _cancelKeyword, ITerminalKeyword.DawnKeywordType.DawnCommand)
                                                .Build();

            _continueNode.clearPreviousText = _parentBuilder._clearTextFlags.HasFlag(ClearText.Continue);
            _cancelNode.clearPreviousText = _parentBuilder._clearTextFlags.HasFlag(ClearText.Cancel);

            return new DawnSimpleQueryCommandInfo(_continueNode, _cancelNode, _continueTerminalKeyword, _cancelTerminalKeyword, _continueCondition, _onQueryContinuedEvent);
        }
    }

    public class SimpleCommandBuilder
    {
        private TerminalCommandInfoBuilder _parentBuilder;

        internal SimpleCommandBuilder(TerminalCommandInfoBuilder parent)
        {
            _parentBuilder = parent;
        }

        internal DawnSimpleCommandInfo Build()
        {
            return new DawnSimpleCommandInfo();
        }
    }

    public class TerminalObjectCommandBuilder
    {
        private TerminalCommandInfoBuilder _parentBuilder;

        internal TerminalObjectCommandBuilder(TerminalCommandInfoBuilder parent)
        {
            _parentBuilder = parent;
        }

        internal DawnTerminalObjectCommandInfo Build()
        {
            return new DawnTerminalObjectCommandInfo();
        }
    }

    public class EventDrivenCommandBuilder
    {
        private TerminalCommandInfoBuilder _parentBuilder;

        internal EventDrivenCommandBuilder(TerminalCommandInfoBuilder parent)
        {
            _parentBuilder = parent;
        }

        internal DawnEventDrivenCommandInfo Build()
        {
            return new DawnEventDrivenCommandInfo();
        }
    }

    public class InputCommandBuilder
    {
        private TerminalCommandInfoBuilder _parentBuilder;

        internal InputCommandBuilder(TerminalCommandInfoBuilder parent)
        {
            _parentBuilder = parent;
        }

        internal DawnInputCommandInfo Build()
        {
            return new DawnInputCommandInfo();
        }
    }
    // Allow setting a query with a continue and cancel word, query text

    public TerminalCommandInfoBuilder DefineQueryCommand(Action<SimpleQueryCommandBuilder> callback)
    {
        SimpleQueryCommandBuilder builder = new(this);
        callback(builder);
        _simpleQueryCommandInfo = builder.Build();
        return this;
    }

    private Func<string> _mainText;
    private string _commandName, _categoryName, _description;
    private ClearText _clearTextFlags;

    public TerminalCommandInfoBuilder(NamespacedKey<DawnTerminalCommandInfo> key, TerminalNode value) : base(key, value)
    {
    }

    public TerminalCommandInfoBuilder SetMainText(Func<string> mainText)
    {
        _mainText = mainText;
        return this;
    }

    public TerminalCommandInfoBuilder SetKeywords(List<string> keywords)
    {

        return this;
    }

    public TerminalCommandInfoBuilder SetClearTextFlags(ClearText clearTextFlags)
    {
        _clearTextFlags = clearTextFlags;
        return this;
    }

    public TerminalCommandInfoBuilder SetCommandName(string commandName)
    {
        _commandName = commandName;
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

    override internal DawnTerminalCommandInfo Build()
    {
        DawnTerminalCommandInfo terminalCommandInfo = new DawnTerminalCommandInfo(key, value, tags, _complexQueryCommandInfo, _simpleQueryCommandInfo, _simpleCommandInfo, _terminalObjectCommandInfo, _eventDrivenCommandInfo, _inputCommandInfo, customData); 
        value.SetDawnInfo(terminalCommandInfo);
        return terminalCommandInfo;
    }
}