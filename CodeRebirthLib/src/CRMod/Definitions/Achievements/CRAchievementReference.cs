using System;

namespace CodeRebirthLib.CRMod;

[Serializable]
public class CRAchievementReference : CRContentReference<CRAchievementDefinition, CRAchievementDefinition>
{
    public CRAchievementReference() : base()
    { }
    public CRAchievementReference(NamespacedKey<CRAchievementDefinition> key) : base(key)
    { }

    public override bool TryResolve(out CRAchievementDefinition info)
    {
        return CRModContent.Achievements.TryGetValue(TypedKey, out info);
    }

    public override CRAchievementDefinition Resolve()
    {
        return CRModContent.Achievements[TypedKey];
    }
}