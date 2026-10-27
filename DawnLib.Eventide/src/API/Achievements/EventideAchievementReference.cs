using System;
using Dawn;
using Dusk;

namespace Eventide;

[Serializable]
public class EventideAchievementReference : DuskContentReference<EventideAchievementDefinition, EventideAchievementDefinition>
{
    public EventideAchievementReference() : base()
    { }

    public EventideAchievementReference(NamespacedKey<EventideAchievementDefinition> key) : base(key)
    { }

    public override bool TryResolve(out EventideAchievementDefinition info)
    {
        return EventideModContent.Achievements.TryGetValue(TypedKey, out info);
    }

    public override EventideAchievementDefinition Resolve()
    {
        return EventideModContent.Achievements[TypedKey];
    }
}