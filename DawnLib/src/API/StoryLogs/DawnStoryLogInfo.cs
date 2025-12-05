using System.Collections.Generic;
using UnityEngine;

namespace Dawn;
public sealed class DawnStoryLogInfo : DawnBaseInfo<DawnStoryLogInfo>
{
    internal DawnStoryLogInfo(NamespacedKey<DawnStoryLogInfo> key, HashSet<NamespacedKey> tags, GameObject storyLogGameObject, TerminalNode storyLogTerminalNode, TerminalKeyword storyLogTerminalKeyword, IDataContainer? customData) : base(key, tags, customData)
    {
        StoryLogGameObject = storyLogGameObject;
        StoryLogTerminalNode = storyLogTerminalNode;
        StoryLogTerminalKeyword = storyLogTerminalKeyword;
    }

    public GameObject StoryLogGameObject { get; }
    public TerminalNode StoryLogTerminalNode { get; }
    public TerminalKeyword StoryLogTerminalKeyword { get; }
}