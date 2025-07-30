using UnityEngine;

namespace CodeRebirthLib.ContentManagement.Achievements;

[CreateAssetMenu(fileName = "New Discovery Achievement Definition", menuName = "CodeRebirthLib/Definitions/Achievements/Discovery Definition")]
public class CRDiscoveryAchievement : CRAchievementBaseDefinition
{
    public bool TriggerAchievement()
    {
        return TryCompleteAchievement();
    }
}