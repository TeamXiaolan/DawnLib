using System;
using System.Collections.Generic;

namespace Dawn;

[Flags]
public enum ClearText
{
    None = 0,
    Result = 1 << 0,
    Query = 1 << 1,
    Cancel = 1 << 2
}

public class TerminalCommandBasicInformation(string commandName, string categoryName, string description, ClearText clearTextFlags)
{
    public string CommandName { get; } = commandName;
    public string CategoryName { get; } = categoryName;
    public string Description { get; } = description;
    public ClearText ClearTextFlags { get; } = clearTextFlags;
}

public class TerminalCommandInfoBuilder : BaseInfoBuilder<DawnTerminalCommandInfo, TerminalCommandBasicInformation, TerminalCommandInfoBuilder>
{
    private DawnComplexQueryCommandInfo? _complexQueryCommandInfo = null;
    private DawnSimpleQueryCommandInfo? _simpleQueryCommandInfo = null;
    private DawnComplexCommandInfo? _complexCommandInfo = null;
    private DawnSimpleCommandInfo? _simpleCommandInfo = null;
    private DawnTerminalObjectCommandInfo? _terminalObjectCommandInfo = null;
    private DawnEventDrivenCommandInfo? _eventDrivenCommandInfo = null;
    private DawnInputCommandInfo? _inputCommandInfo = null;

    private List<string> _keywords = new();

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

        private Func<string> _resultDisplayText;
        private string _continueOrCancelDisplayText, _cancelDisplayText;
        private string _continueKeyword, _cancelKeyword;
        private Func<bool> _continueCondition;
        private Action<bool>? _onQueryContinuedEvent;

        internal SimpleQueryCommandBuilder(TerminalCommandInfoBuilder parent)
        {
            _parentBuilder = parent;
        }

        public SimpleQueryCommandBuilder SetResult(Func<string> resultDisplayText)
        {
            _resultDisplayText = resultDisplayText;
            return this;
        }

        public SimpleQueryCommandBuilder SetContinueOrCancel(string continueOrCancelDisplayText)
        {
            _continueOrCancelDisplayText = continueOrCancelDisplayText;
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

            TerminalNode _resultNode = new TerminalNodeBuilder($"{_parentBuilder.key}:ResultNode")
                                            .SetDisplayText(_resultDisplayText.Invoke())
                                            .SetClearPreviousText(_parentBuilder.value.ClearTextFlags.HasFlag(ClearText.Result))
                                            .SetMaxCharactersToType(35)
                                            .Build();

            _resultNode.SetDynamicDisplayText(_resultDisplayText);

            TerminalNode _continueOrCancelNode = new TerminalNodeBuilder($"{_parentBuilder.key}:ContinueNode")
                                            .SetDisplayText(_continueOrCancelDisplayText)
                                            .SetClearPreviousText(_parentBuilder.value.ClearTextFlags.HasFlag(ClearText.Query))
                                            .SetMaxCharactersToType(35)
                                            .Build();
            
            TerminalNode _cancelNode = new TerminalNodeBuilder($"{_parentBuilder.key}:CancelNode")
                                            .SetDisplayText(_cancelDisplayText)
                                            .SetClearPreviousText(_parentBuilder.value.ClearTextFlags.HasFlag(ClearText.Cancel))
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

            TerminalKeyword _continueTerminalKeyword = new TerminalKeywordBuilder($"{_parentBuilder.key}:ContinueKeyword", _continueKeyword)
                                                .Build();
            
            TerminalKeyword _cancelTerminalKeyword = new TerminalKeywordBuilder($"{_parentBuilder.key}:CancelKeyword", _cancelKeyword)
                                                .Build();


            return new DawnSimpleQueryCommandInfo(_resultNode, _continueOrCancelNode, _cancelNode, _continueTerminalKeyword, _cancelTerminalKeyword, _continueCondition, _onQueryContinuedEvent);
        }
    }

    public class ComplexCommandBuilder
    {
        private TerminalCommandInfoBuilder _parentBuilder;

        internal ComplexCommandBuilder(TerminalCommandInfoBuilder parent)
        {
            _parentBuilder = parent;
        }

        internal DawnComplexCommandInfo Build()
        {
            return new DawnComplexCommandInfo();
        }
    }

    public class SimpleCommandBuilder
    {
        private TerminalCommandInfoBuilder _parentBuilder;

        private Func<string> _resultDisplayText;
        internal SimpleCommandBuilder(TerminalCommandInfoBuilder parent)
        {
            _parentBuilder = parent;
        }

        public SimpleCommandBuilder SetResult(Func<string> resultDisplayText)
        {
            _resultDisplayText = resultDisplayText;
            return this;
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

    public TerminalCommandInfoBuilder DefineComplexQueryCommand(Action<ComplexQueryCommandBuilder> callback)
    {
        ComplexQueryCommandBuilder builder = new(this);
        callback(builder);
        _complexQueryCommandInfo = builder.Build();
        return this;
    }

    public TerminalCommandInfoBuilder DefineSimpleQueryCommand(Action<SimpleQueryCommandBuilder> callback)
    {
        SimpleQueryCommandBuilder builder = new(this);
        callback(builder);
        _simpleQueryCommandInfo = builder.Build();
        return this;
    }

    public TerminalCommandInfoBuilder DefineComplexCommand(Action<ComplexCommandBuilder> callback)
    {
        ComplexCommandBuilder builder = new(this);
        callback(builder);
        _complexCommandInfo = builder.Build();
        return this;
    }

    public TerminalCommandInfoBuilder DefineSimpleCommand(Action<SimpleCommandBuilder> callback)
    {
        SimpleCommandBuilder builder = new(this);
        callback(builder);
        _simpleCommandInfo = builder.Build();
        return this;
    }

    public TerminalCommandInfoBuilder DefineTerminalObjectCommand(Action<TerminalObjectCommandBuilder> callback)
    {
        TerminalObjectCommandBuilder builder = new(this);
        callback(builder);
        _terminalObjectCommandInfo = builder.Build();
        return this;
    }

    public TerminalCommandInfoBuilder DefineEventDrivenCommand(Action<EventDrivenCommandBuilder> callback)
    {
        EventDrivenCommandBuilder builder = new(this);
        callback(builder);
        _eventDrivenCommandInfo = builder.Build();
        return this;
    }

    public TerminalCommandInfoBuilder DefineInputCommand(Action<InputCommandBuilder> callback)
    {
        InputCommandBuilder builder = new(this);
        callback(builder);
        _inputCommandInfo = builder.Build();
        return this;
    }

    public TerminalCommandInfoBuilder(NamespacedKey<DawnTerminalCommandInfo> key, TerminalCommandBasicInformation value) : base(key, value)
    {
    }

    public TerminalCommandInfoBuilder SetKeywords(List<string> keywords)
    {
        _keywords = keywords;
        return this;
    }

    override internal DawnTerminalCommandInfo Build()
    {
        if (_keywords.Count <= 0)
        {
            throw new ArgumentException($"Command: '{key}' didn't set any keywords, this will cause issues.");
        }

        List<TerminalKeyword> terminalKeywords = new();

        foreach (string keyword in _keywords)
        {
            TerminalKeyword terminalKeyword = new TerminalKeywordBuilder($"{key}_{keyword}", keyword, ITerminalKeyword.DawnKeywordType.DawnCommand)
                                                .Build();

            terminalKeywords.Add(terminalKeyword);
        }

        return new DawnTerminalCommandInfo(key, value, terminalKeywords, tags, _complexQueryCommandInfo, _simpleQueryCommandInfo, _complexCommandInfo, _simpleCommandInfo, _terminalObjectCommandInfo, _eventDrivenCommandInfo, _inputCommandInfo, customData);;
    }
}