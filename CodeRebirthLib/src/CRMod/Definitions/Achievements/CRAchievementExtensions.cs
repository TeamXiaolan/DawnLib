using System.Collections.Generic;
using CodeRebirthLib.CRMod;

namespace CodeRebirthLib;

public static class CRAchievementExtensions
{
    public static bool TryTriggerAchievement(this Registry<CRAchievementDefinition> registry, NamespacedKey<CRAchievementDefinition> achievementKey)
    {
        return registry.TryGetValue(achievementKey, out CRAchievementDefinition? value) && value is CRInstantAchievement instant && instant.TriggerAchievement();
    }

    public static bool TryIncrementAchievement(this Registry<CRAchievementDefinition> registry, NamespacedKey<CRAchievementDefinition> achievementKey, float amount)
    {
        return registry.TryGetValue(achievementKey, out CRAchievementDefinition? value) && value is CRStatAchievement progressive && progressive.IncrementProgress(amount);
    }

    public static bool TryDiscoverMoreProgressAchievement(this Registry<CRAchievementDefinition> registry, NamespacedKey<CRAchievementDefinition> achievementKey, IEnumerable<string> uniqueStringIDs)
    {
        return registry.TryGetValue(achievementKey, out CRAchievementDefinition? value) && value is CRDiscoveryAchievement discovery && discovery.TryDiscoverMoreProgress(uniqueStringIDs);
    }

    public static bool TryDiscoverMoreProgressAchievement(this Registry<CRAchievementDefinition> registry, NamespacedKey<CRAchievementDefinition> achievementKey, string uniqueStringID)
    {
        return registry.TryGetValue(achievementKey, out CRAchievementDefinition? value) && value is CRDiscoveryAchievement discovery && discovery.TryDiscoverMoreProgress(uniqueStringID);
    }

    public static void ResetAchievementProgress(this Registry<CRAchievementDefinition> registry, NamespacedKey<CRAchievementDefinition> achievementKey)
    {
        if (registry.TryGetValue(achievementKey, out CRAchievementDefinition? value))
        {
            value.ResetProgress();
        }
    }
}