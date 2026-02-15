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

[Serializable]
public class TerminalCommandBasicInformation(string commandName, string categoryName, string description, ClearText clearTextFlags)
{
    public string CommandName { get; private set; } = commandName;
    public string CategoryName { get; private set; } = categoryName;
    public string Description { get; private set; } = description;
    public ClearText ClearTextFlags { get; private set; } = clearTextFlags;
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

        private List<Func<string>> _resultDisplayTexts;
        private Func<string> _continueOrCancelDisplayText;
        private Func<string> _cancelDisplayText;
        private string _cancelKeyword;
        private List<string> _continueKeywords;
        private List<Func<bool>> _continueConditions = new();
        private List<Action<bool>> _onQueryContinuedEvents = new();

        internal ComplexQueryCommandBuilder(TerminalCommandInfoBuilder parent)
        {
            _parentBuilder = parent;
        }

        public ComplexQueryCommandBuilder SetResultDisplayTexts(List<Func<string>> resultDisplayTexts)
        {
            _resultDisplayTexts = resultDisplayTexts;
            return this;
        }

        public ComplexQueryCommandBuilder SetContinueOrCancelDisplayTexts(Func<string> continueOrCancelDisplayText)
        {
            _continueOrCancelDisplayText = continueOrCancelDisplayText;
            return this;
        }

        public ComplexQueryCommandBuilder SetCancelDisplayText(Func<string> cancelDisplayText)
        {
            _cancelDisplayText = cancelDisplayText;
            return this;
        }

        public ComplexQueryCommandBuilder SetContinueKeywords(List<string> continueKeywords)
        {
            _continueKeywords = continueKeywords;
            return this;
        }

        public ComplexQueryCommandBuilder SetCancelKeyword(string cancelKeyword)
        {
            _cancelKeyword = cancelKeyword;
            return this;
        }

        public ComplexQueryCommandBuilder SetContinueConditions(List<Func<bool>> continueConditions)
        {
            _continueConditions = continueConditions;
            return this;
        }

        public ComplexQueryCommandBuilder SetOnQueryContinuedEvents(List<Action<bool>> onQueryContinuedEvents)
        {
            _onQueryContinuedEvents = onQueryContinuedEvents;
            return this;
        }

        internal DawnComplexQueryCommandInfo Build()
        {
            TerminalNode continueOrCancelNode =  new TerminalNodeBuilder($"{_parentBuilder.key}:ContinueNode")
                                                .SetDisplayText(_continueOrCancelDisplayText.Invoke())
                                                .SetClearPreviousText(_parentBuilder.value.ClearTextFlags.HasFlag(ClearText.Query))
                                                .SetMaxCharactersToType(35)
                                                .Build();

            continueOrCancelNode.SetDynamicDisplayText(_continueOrCancelDisplayText);

            TerminalNode cancelNode = new TerminalNodeBuilder($"{_parentBuilder.key}:CancelNode")
                                            .SetDisplayText(_cancelDisplayText.Invoke())
                                            .SetClearPreviousText(_parentBuilder.value.ClearTextFlags.HasFlag(ClearText.Cancel))
                                            .SetMaxCharactersToType(35)
                                            .Build();

            cancelNode.SetDynamicDisplayText(_cancelDisplayText);;

            if (string.IsNullOrEmpty(_cancelKeyword))
            {
                _cancelKeyword = "cancel";
                DawnPlugin.Logger.LogWarning($"Query '{_parentBuilder.key}' didn't set cancel word. Defaulting to '{_cancelKeyword}'");
            }

            TerminalKeyword cancelTerminalKeyword = new TerminalKeywordBuilder($"{_parentBuilder.key}:CancelKeyword", _cancelKeyword)
                                                .Build();

            List<TerminalNode> resultNodes = new();
            List<TerminalKeyword> continueTerminalKeywords = new();

            for (int i = 0; i < _resultDisplayTexts.Count; i++)
            {
                TerminalNode _resultNode = new TerminalNodeBuilder($"{_parentBuilder.key}:ResultNode")
                                                .SetDisplayText(_resultDisplayTexts[i].Invoke())
                                                .SetClearPreviousText(_parentBuilder.value.ClearTextFlags.HasFlag(ClearText.Result))
                                                .SetMaxCharactersToType(35)
                                                .Build();

                _resultNode.SetDynamicDisplayText(_resultDisplayTexts[i]);
                resultNodes.Add(_resultNode);

                if (_continueConditions.Count <= i)
                {
                    _continueConditions.Add(() => true);
                }

                if (_onQueryContinuedEvents.Count <= i)
                {
                    _onQueryContinuedEvents.Add(_ => { });
                }

                if (_continueKeywords.Count <= i)
                {
                    _continueKeywords.Add("continue");
                    DawnPlugin.Logger.LogWarning($"Query '{_parentBuilder.key}' didn't set continue word. Defaulting to '{_continueKeywords}'");
                }

                TerminalKeyword _continueTerminalKeyword = new TerminalKeywordBuilder($"{_parentBuilder.key}:ContinueKeyword", _continueKeywords[i])
                                                    .Build();

                continueTerminalKeywords.Add(_continueTerminalKeyword);
            }

            return new DawnComplexQueryCommandInfo(resultNodes, continueOrCancelNode, cancelNode, continueTerminalKeywords, cancelTerminalKeyword, _continueConditions, _onQueryContinuedEvents);
        }
    }

    public class SimpleQueryCommandBuilder
    {
        private TerminalCommandInfoBuilder _parentBuilder;

        private Func<string> _resultDisplayText;
        private Func<string> _continueOrCancelDisplayText, _cancelDisplayText;
        private string _continueKeyword, _cancelKeyword;
        private Func<bool> _continueCondition;
        private Action<bool> _onQueryContinuedEvent;

        internal SimpleQueryCommandBuilder(TerminalCommandInfoBuilder parent)
        {
            _parentBuilder = parent;
        }

        public SimpleQueryCommandBuilder SetResult(Func<string> resultDisplayText)
        {
            _resultDisplayText = resultDisplayText;
            return this;
        }

        public SimpleQueryCommandBuilder SetContinueOrCancel(Func<string> continueOrCancelDisplayText)
        {
            _continueOrCancelDisplayText = continueOrCancelDisplayText;
            return this;
        }

        public SimpleQueryCommandBuilder SetCancel(Func<string> cancelDisplayText)
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

            if (_onQueryContinuedEvent == null)
            {
                _onQueryContinuedEvent = new Action<bool>(_ => { });
            }

            TerminalNode _resultNode = new TerminalNodeBuilder($"{_parentBuilder.key}:ResultNode")
                                            .SetDisplayText(_resultDisplayText.Invoke())
                                            .SetClearPreviousText(_parentBuilder.value.ClearTextFlags.HasFlag(ClearText.Result))
                                            .SetMaxCharactersToType(35)
                                            .Build();

            _resultNode.SetDynamicDisplayText(_resultDisplayText);

            TerminalNode _continueOrCancelNode = new TerminalNodeBuilder($"{_parentBuilder.key}:ContinueNode")
                                            .SetDisplayText(_continueOrCancelDisplayText.Invoke())
                                            .SetClearPreviousText(_parentBuilder.value.ClearTextFlags.HasFlag(ClearText.Query))
                                            .SetMaxCharactersToType(35)
                                            .Build();

            _continueOrCancelNode.SetDynamicDisplayText(_continueOrCancelDisplayText);

            TerminalNode _cancelNode = new TerminalNodeBuilder($"{_parentBuilder.key}:CancelNode")
                                            .SetDisplayText(_cancelDisplayText.Invoke())
                                            .SetClearPreviousText(_parentBuilder.value.ClearTextFlags.HasFlag(ClearText.Cancel))
                                            .SetMaxCharactersToType(35)
                                            .Build();

            _cancelNode.SetDynamicDisplayText(_cancelDisplayText);

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

        private List<string> _secondaryKeywords = new();
        private List<Func<string>> _resultDisplayTexts = new();
        internal ComplexCommandBuilder(TerminalCommandInfoBuilder parent)
        {
            _parentBuilder = parent;
        }

        public ComplexCommandBuilder SetSecondaryKeywords(List<string> secondaryKeywords)
        {
            _secondaryKeywords = secondaryKeywords;
            return this;
        }

        public ComplexCommandBuilder SetResultsDisplayText(List<Func<string>> resultDisplayTexts)
        {
            _resultDisplayTexts = resultDisplayTexts;
            return this;
        }

        internal DawnComplexCommandInfo Build()
        {
            if (_secondaryKeywords.Count <= 0)
            {
                throw new ArgumentException($"Command: '{_parentBuilder.key}' didn't set any keywords, this will cause issues.");
            }

            if (_secondaryKeywords.Count != _resultDisplayTexts.Count)
            {
                throw new ArgumentException($"Command: '{_parentBuilder.key}' didn't set the same amount of keywords as results.");
            }

            List<TerminalNode> resultNodes = new();
            List<TerminalKeyword> secondaryTerminalKeywords = new();

            foreach (string secondaryKeyword in _secondaryKeywords)
            {
                TerminalKeyword terminalKeyword = new TerminalKeywordBuilder($"{_parentBuilder.key}:{secondaryKeyword}", secondaryKeyword)
                                                    .Build();

                secondaryTerminalKeywords.Add(terminalKeyword);
            }

            foreach (Func<string> resultDisplayText in _resultDisplayTexts)
            {
                TerminalNode resultNode = new TerminalNodeBuilder($"{_parentBuilder.key}:ResultNode")
                                                .SetDisplayText(resultDisplayText.Invoke())
                                                .Build();

                resultNode.SetDynamicDisplayText(resultDisplayText);
                resultNodes.Add(resultNode);
            }
            return new DawnComplexCommandInfo(resultNodes, secondaryTerminalKeywords);
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

        public SimpleCommandBuilder SetResultDisplayText(Func<string> resultDisplayText)
        {
            _resultDisplayText = resultDisplayText;
            return this;
        }

        internal DawnSimpleCommandInfo Build()
        {
            TerminalNode resultNode = new TerminalNodeBuilder($"{_parentBuilder.key}:ResultNode")
                                            .SetDisplayText(_resultDisplayText.Invoke())
                                            .Build();

            resultNode.SetDynamicDisplayText(_resultDisplayText);
            return new DawnSimpleCommandInfo(resultNode);
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

        private Func<string> _resultNodeDisplayText;
        private TerminalNode _resultNode;
        private Action<Terminal, TerminalNode> _onTerminalEvent;

        internal EventDrivenCommandBuilder(TerminalCommandInfoBuilder parent)
        {
            _parentBuilder = parent;
        }

        public EventDrivenCommandBuilder SetResultNodeDisplayText(Func<string> resultNodeDisplayText)
        {
            _resultNodeDisplayText = resultNodeDisplayText;
            return this;
        }

        public EventDrivenCommandBuilder OverrideResultNode(TerminalNode resultNode)
        {
            _resultNode = resultNode;
            return this;
        }

        public EventDrivenCommandBuilder SetOnTerminalEvent(Action<Terminal, TerminalNode> onTerminalEvent)
        {
            _onTerminalEvent = onTerminalEvent;
            return this;
        }

        internal DawnEventDrivenCommandInfo Build()
        {
            if (_resultNode == null)
            {
                _resultNode = new TerminalNodeBuilder($"{_parentBuilder.key}:ResultNode")
                                                    .SetDisplayText(_resultNodeDisplayText.Invoke())
                                                    .Build();

                _resultNode.SetDynamicDisplayText(_resultNodeDisplayText);
            }

            if (_onTerminalEvent == null)
            {
                throw new ArgumentException($"Event driven command: '{_parentBuilder.key}' didn't set onTerminalEvent.");
            }

            return new DawnEventDrivenCommandInfo(_resultNode, _onTerminalEvent);
        }
    }

    public class InputCommandBuilder
    {
        private TerminalCommandInfoBuilder _parentBuilder;

        private Func<string, string> _resultDisplayText;

        internal InputCommandBuilder(TerminalCommandInfoBuilder parent)
        {
            _parentBuilder = parent;
        }

        public InputCommandBuilder SetResultDisplayText(Func<string, string> resultDisplayText)
        {
            _resultDisplayText = resultDisplayText;
            return this;
        }

        internal DawnInputCommandInfo Build()
        {
            if (_resultDisplayText == null)
            {
                throw new ArgumentException($"Input command: '{_parentBuilder.key}' didn't set result display text.");
            }

            TerminalNode resultNode = new TerminalNodeBuilder($"{_parentBuilder.key}:ResultNode")
                                            .SetDisplayText(_resultDisplayText.Invoke(DawnInputCommandInfo.GetLastUserInput()))
                                            .Build();

            return new DawnInputCommandInfo(resultNode, _resultDisplayText);
        }
    }

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