using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CodeRebirthLib.Extensions;

namespace CodeRebirthLib.ContentManagement.Achievements;
public static class CRModAchievementExtensions
{
    public static bool TryGetFromAchievementName(this IEnumerable<CRAchievementBaseDefinition> registry, string achievementName, [NotNullWhen(true)] out CRAchievementBaseDefinition? value)
    {
        return registry.TryGetFirstBySomeName(it => it.AchievementName,
            achievementName,
            out value,
            $"TryGetFromAchievementName failed with achievementName: {achievementName}"
        );
    }

    public static bool TryTriggerAchievement(this IEnumerable<CRAchievementBaseDefinition> registry, string achievementName)
    {
        // todo: CodeRebirthLibPlugin.Logger.LogError($"Trying to complete achievement: {_achievementName} but could not be found. Or it is not a CRInstantAchievement");
        return registry.TryGetFromAchievementName(achievementName, out CRAchievementBaseDefinition? value) && value is CRInstantAchievement instant && instant.TriggerAchievement();
    }

    public static bool TryIncrementAchievement(this IEnumerable<CRAchievementBaseDefinition> registry, string achievementName, float amount)
    {
        return registry.TryGetFromAchievementName(achievementName, out CRAchievementBaseDefinition? value) && value is CRProgressiveAchievement progressive && progressive.IncrementProgress(amount);
    }
}