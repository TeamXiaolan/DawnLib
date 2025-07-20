using UnityEngine;

namespace CodeRebirthLib.ContentManagement.Achievements;

[CreateAssetMenu(fileName = "New Instant Achievement Definition", menuName = "CodeRebirthLib/Definitions/Instant Achievement Definition")]
public class CRInstantAchievement : CRAchievementBaseDefinition
{
    public override bool TryCompleteAchievement()
    {
        if (Completed)
        {
            return false;
        }

        Completed = true;
        return Completed;
    }
}