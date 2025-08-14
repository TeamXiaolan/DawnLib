using System.Collections.Generic;
using CodeRebirthLib.CRMod;

namespace CodeRebirthLib;

public static class CRAchievementExtensions
{
    public static bool TryTriggerAchievement(this Registry<CRMAchievementDefinition> registry, NamespacedKey<CRMAchievementDefinition> achievementKey)
    {
        return registry.TryGetValue(achievementKey, out CRMAchievementDefinition? value) && value is CRMInstantAchievement instant && instant.TriggerAchievement();
    }

    public static bool TryIncrementAchievement(this Registry<CRMAchievementDefinition> registry, NamespacedKey<CRMAchievementDefinition> achievementKey, float amount)
    {
        return registry.TryGetValue(achievementKey, out CRMAchievementDefinition? value) && value is CRMStatAchievement progressive && progressive.IncrementProgress(amount);
    }

    public static bool TryDiscoverMoreProgressAchievement(this Registry<CRMAchievementDefinition> registry, NamespacedKey<CRMAchievementDefinition> achievementKey, IEnumerable<string> uniqueStringIDs)
    {
        return registry.TryGetValue(achievementKey, out CRMAchievementDefinition? value) && value is CRMDiscoveryAchievement discovery && discovery.TryDiscoverMoreProgress(uniqueStringIDs);
    }

    public static bool TryDiscoverMoreProgressAchievement(this Registry<CRMAchievementDefinition> registry, NamespacedKey<CRMAchievementDefinition> achievementKey, string uniqueStringID)
    {
        return registry.TryGetValue(achievementKey, out CRMAchievementDefinition? value) && value is CRMDiscoveryAchievement discovery && discovery.TryDiscoverMoreProgress(uniqueStringID);
    }

    public static void ResetAchievementProgress(this Registry<CRMAchievementDefinition> registry, NamespacedKey<CRMAchievementDefinition> achievementKey)
    {
        if (registry.TryGetValue(achievementKey, out CRMAchievementDefinition? value))
        {
            value.ResetProgress();
        }
    }
}