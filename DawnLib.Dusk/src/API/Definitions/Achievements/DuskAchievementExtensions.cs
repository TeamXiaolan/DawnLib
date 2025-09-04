using System.Collections.Generic;
using Dawn;

namespace Dusk;

public static class DuskAchievementExtensions
{
    public static bool TryTriggerAchievement(this Registry<DuskAchievementDefinition> registry, NamespacedKey<DuskAchievementDefinition> achievementKey)
    {
        return registry.TryGetValue(achievementKey, out DuskAchievementDefinition? value) && value is DuskInstantAchievement instant && instant.TriggerAchievement();
    }

    public static bool TryIncrementAchievement(this Registry<DuskAchievementDefinition> registry, NamespacedKey<DuskAchievementDefinition> achievementKey, float amount)
    {
        return registry.TryGetValue(achievementKey, out DuskAchievementDefinition? value) && value is DuskStatAchievement progressive && progressive.IncrementProgress(amount);
    }

    public static bool TryDiscoverMoreProgressAchievement(this Registry<DuskAchievementDefinition> registry, NamespacedKey<DuskAchievementDefinition> achievementKey, IEnumerable<string> uniqueStringIDs)
    {
        return registry.TryGetValue(achievementKey, out DuskAchievementDefinition? value) && value is DuskDiscoveryAchievement discovery && discovery.TryDiscoverMoreProgress(uniqueStringIDs);
    }

    public static bool TryDiscoverMoreProgressAchievement(this Registry<DuskAchievementDefinition> registry, NamespacedKey<DuskAchievementDefinition> achievementKey, string uniqueStringID)
    {
        return registry.TryGetValue(achievementKey, out DuskAchievementDefinition? value) && value is DuskDiscoveryAchievement discovery && discovery.TryDiscoverMoreProgress(uniqueStringID);
    }

    public static void ResetAchievementProgress(this Registry<DuskAchievementDefinition> registry, NamespacedKey<DuskAchievementDefinition> achievementKey)
    {
        if (registry.TryGetValue(achievementKey, out DuskAchievementDefinition? value))
        {
            value.ResetProgress();
        }
    }
}