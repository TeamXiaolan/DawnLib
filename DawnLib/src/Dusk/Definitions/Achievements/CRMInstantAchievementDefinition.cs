
using UnityEngine;

namespace Dawn.Dusk;

[CreateAssetMenu(fileName = "New Instant Achievement Definition", menuName = $"{CRModConstants.Achievements}/Instant Definition")]
public class CRMInstantAchievement : CRMAchievementDefinition
{
    public bool TriggerAchievement()
    {
        return TryCompleteAchievement();
    }
}
