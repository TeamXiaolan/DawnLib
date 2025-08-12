
using UnityEngine;

namespace CodeRebirthLib;

[CreateAssetMenu(fileName = "New Instant Achievement Definition", menuName = "CodeRebirthLib/Definitions/Achievements/Instant Definition")]
public class CRInstantAchievement : CRAchievementDefinition
{
    public bool TriggerAchievement()
    {
        return TryCompleteAchievement();
    }
}
