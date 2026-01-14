using System;
using Dawn;

namespace Dusk;

[Serializable]
public class DuskStoryLogReference : DuskContentReference<DuskStoryLogDefinition, DawnStoryLogInfo>
{
    public DuskStoryLogReference() : base()
    { }
    public DuskStoryLogReference(NamespacedKey<DawnStoryLogInfo> key) : base(key)
    { }
    public override bool TryResolve(out DawnStoryLogInfo info)
    {
        return LethalContent.StoryLogs.TryGetValue(TypedKey, out info);
    }

    public override DawnStoryLogInfo Resolve()
    {
        return LethalContent.StoryLogs[TypedKey];
    }
}