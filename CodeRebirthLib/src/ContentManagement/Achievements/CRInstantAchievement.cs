using UnityEngine;

namespace CodeRebirthLib.ContentManagement.Achievements;

[CreateAssetMenu(fileName = "New Instant Achievement Definition", menuName = "CodeRebirthLib/Definitions/Achievements/Instant Definition")]
public class CRInstantAchievement : CRAchievementBaseDefinition
{
    public bool TriggerAchievement()
    {
        return TryCompleteAchievement();
    }
}