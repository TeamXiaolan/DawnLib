using System;

namespace Dawn.Dusk;

[Serializable]
public class CRMAchievementReference : CRMContentReference<CRMAchievementDefinition, CRMAchievementDefinition>
{
    public CRMAchievementReference() : base()
    { }

    public CRMAchievementReference(NamespacedKey<CRMAchievementDefinition> key) : base(key)
    { }

    public override bool TryResolve(out CRMAchievementDefinition info)
    {
        return CRModContent.Achievements.TryGetValue(TypedKey, out info);
    }

    public override CRMAchievementDefinition Resolve()
    {
        return CRModContent.Achievements[TypedKey];
    }
}