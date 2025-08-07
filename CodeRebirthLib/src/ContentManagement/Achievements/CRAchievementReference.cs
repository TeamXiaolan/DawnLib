using System;

namespace CodeRebirthLib.ContentManagement.Achievements;

[Serializable]
public class CRAchievementReference(string name) : CRContentReference<CRAchievementBaseDefinition>(name)
{
    protected override string GetEntityName(CRAchievementBaseDefinition obj) => obj.AchievementName;

    public static implicit operator CRAchievementBaseDefinition?(CRAchievementReference reference)
    {
        if (CRMod.AllAchievements().TryGetFromAchievementName(reference.entityName, out var achievement))
        {
            return achievement;
        }
        return null;
    }

    public static implicit operator CRAchievementReference?(CRAchievementBaseDefinition? obj)
    {
        if (obj) return new CRAchievementReference(obj!.AchievementName);
        return null;
    }
}