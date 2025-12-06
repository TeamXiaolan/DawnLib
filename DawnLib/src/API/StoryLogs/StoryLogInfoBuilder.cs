using UnityEngine;

namespace Dawn;

public class StoryLogInfoBuilder : BaseInfoBuilder<DawnStoryLogInfo, GameObject, StoryLogInfoBuilder>
{
    private TerminalNode? _storyLogTerminalNode = null;

    private string _storyLogTitle, _storyLogKeyword;
    private string _storyLogDescription = "The developer of this story log left this empty!";
    public StoryLogInfoBuilder(NamespacedKey<DawnStoryLogInfo> key, GameObject value) : base(key, value)
    {
    }

    public StoryLogInfoBuilder OverrideTerminalNode(TerminalNode terminalNode)
    {
        _storyLogTerminalNode = terminalNode;
        return this;
    }

    public StoryLogInfoBuilder SetDescription(string description)
    {
        _storyLogDescription = description;
        return this;
    }

    public StoryLogInfoBuilder SetTitle(string title)
    {
        _storyLogTitle = title;
        return this;
    }

    public StoryLogInfoBuilder SetKeyword(string keyword)
    {
        _storyLogKeyword = keyword.ToLowerInvariant();
        return this;
    }

    override internal DawnStoryLogInfo Build()
    {
        if (_storyLogTerminalNode == null)
        {
            _storyLogTerminalNode = new TerminalNodeBuilder($"{key}TerminalNode")
                                        .SetClearPreviousText(true)
                                        .SetCreatureName(_storyLogTitle)
                                        .SetMaxCharactersToType(35)
                                        .SetDisplayText(_storyLogDescription)
                                        .Build();
        }

        if (string.IsNullOrEmpty(_storyLogKeyword))
        {
            DawnPlugin.Logger.LogWarning($"StoryLog: '{key}' didn't set keyword, please call .SetKeyword on the builder.");
        }

        TerminalKeyword storyLogTerminalKeyword = new TerminalKeywordBuilder($"{key}TerminalKeyword")
                                    .SetWord(_storyLogKeyword)
                                    .Build();

        return new DawnStoryLogInfo(key, tags, value, _storyLogTerminalNode, storyLogTerminalKeyword, customData);
    }
}