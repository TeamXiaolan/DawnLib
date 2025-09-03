
using UnityEngine;

namespace Dawn.Dusk;

[CreateAssetMenu(fileName = "New Instant Achievement Definition", menuName = $"{DuskModConstants.Achievements}/Instant Definition")]
public class DuskInstantAchievement : DuskAchievementDefinition
{
    public bool TriggerAchievement()
    {
        return TryCompleteAchievement();
    }
}
