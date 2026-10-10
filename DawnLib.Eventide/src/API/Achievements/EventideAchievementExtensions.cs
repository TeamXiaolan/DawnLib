using System.Collections.Generic;
using Dawn;

namespace Eventide;

public static class EventideAchievementExtensions
{
    public static bool TryTriggerAchievement(this Registry<EventideAchievementDefinition> registry, NamespacedKey<EventideAchievementDefinition> achievementKey)
    {
        return registry.TryGetValue(achievementKey, out EventideAchievementDefinition? value) && value is EventideInstantAchievement instant && instant.TriggerAchievement();
    }

    public static bool TryIncrementAchievement(this Registry<EventideAchievementDefinition> registry, NamespacedKey<EventideAchievementDefinition> achievementKey, float amount)
    {
        return registry.TryGetValue(achievementKey, out EventideAchievementDefinition? value) && value is EventideStatAchievement progressive && progressive.IncrementProgress(amount);
    }

    public static bool TryDiscoverMoreProgressAchievement(this Registry<EventideAchievementDefinition> registry, NamespacedKey<EventideAchievementDefinition> achievementKey, IEnumerable<string> uniqueStringIDs)
    {
        return registry.TryGetValue(achievementKey, out EventideAchievementDefinition? value) && value is EventideDiscoveryAchievement discovery && discovery.TryDiscoverMoreProgress(uniqueStringIDs);
    }

    public static bool TryDiscoverMoreProgressAchievement(this Registry<EventideAchievementDefinition> registry, NamespacedKey<EventideAchievementDefinition> achievementKey, string uniqueStringID)
    {
        return registry.TryGetValue(achievementKey, out EventideAchievementDefinition? value) && value is EventideDiscoveryAchievement discovery && discovery.TryDiscoverMoreProgress(uniqueStringID);
    }

    public static void ResetAchievement(this Registry<EventideAchievementDefinition> registry, NamespacedKey<EventideAchievementDefinition> achievementKey)
    {
        if (registry.TryGetValue(achievementKey, out EventideAchievementDefinition? value))
        {
            value.ResetProgress();
        }
    }

    public static void SoftResetAchievement(this Registry<EventideAchievementDefinition> registry, NamespacedKey<EventideAchievementDefinition> achievementKey)
    {
        if (registry.TryGetValue(achievementKey, out EventideAchievementDefinition? value))
        {
            value.SoftResetProgress();
        }
    }
}