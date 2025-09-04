using System;
using Dawn;

namespace Dusk;

[Serializable]
public class DuskAchievementReference : DuskContentReference<DuskAchievementDefinition, DuskAchievementDefinition>
{
    public DuskAchievementReference() : base()
    { }

    public DuskAchievementReference(NamespacedKey<DuskAchievementDefinition> key) : base(key)
    { }

    public override bool TryResolve(out DuskAchievementDefinition info)
    {
        return DuskModContent.Achievements.TryGetValue(TypedKey, out info);
    }

    public override DuskAchievementDefinition Resolve()
    {
        return DuskModContent.Achievements[TypedKey];
    }
}