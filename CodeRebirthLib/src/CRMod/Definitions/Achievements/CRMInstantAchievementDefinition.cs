
using UnityEngine;

namespace CodeRebirthLib.CRMod;

[CreateAssetMenu(fileName = "New Instant Achievement Definition", menuName = $"{CRModConstants.Achievements}/Instant Definition")]
public class CRMInstantAchievement : CRMAchievementDefinition
{
    public bool TriggerAchievement()
    {
        return TryCompleteAchievement();
    }
}
