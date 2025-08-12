using System;

namespace CodeRebirthLib.CRMod;

[Serializable]
public class CRAchievementReference : CRContentReference<CRAchievementDefinition, CRAchievementInfo>
{
    public CRAchievementReference() : base()
    { }
    public CRAchievementReference(NamespacedKey<CRAchievementInfo> key) : base(key) 
    { }

    public override bool TryResolve(out CRAchievementInfo info)
    {
        return CRLibContent.Achievements.TryGetValue(TypedKey, out info);
    }
}