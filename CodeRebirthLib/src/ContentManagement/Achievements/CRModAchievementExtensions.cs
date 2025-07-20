using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CodeRebirthLib.Extensions;

namespace CodeRebirthLib.ContentManagement.Achievements;
public static class CRModAchievementExtensions
{
    public static bool TryGetFromAchievementName(this IEnumerable<CRAchievementBaseDefinition> registry, string achievementName, [NotNullWhen(true)] out CRAchievementBaseDefinition? value)
    {
        return CRRegistryExtensions.TryGetFirstBySomeName(registry, 
            it => it.AchievementName,
            achievementName,
            out value,
            $"TryGetFromAchievementName failed with achievementName: {achievementName}"
        );
    }

    /*public static bool TryGetDefinition(this AchievementType type, [NotNullWhen(true)] out CRAchievementBaseDefinition? definition)
    {
        definition = CRMod.AllAchievements().FirstOrDefault(it => it.AchievementType == type);
        if (!definition) CodeRebirthLibPlugin.ExtendedLogging($"TryGetDefinition for CRAchievementBaseDefinition failed with {type.achievementName}");
        return definition; // implict cast
    }*/
}