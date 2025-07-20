using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace CodeRebirthLib.ContentManagement.Achievements;

public class AchievementTriggers : NetworkBehaviour
{
    [SerializeField]
    private string _achievementName = string.Empty;
    public UnityEvent _onAchievementCompleted = new UnityEvent();

    public void TryCompleteAchievement()
    {
        if (!CRMod.AllAchievements().TryGetFromAchievementName(_achievementName, out CRAchievementBaseDefinition? achievementBaseDefinition) || achievementBaseDefinition is not CRInstantAchievement instantAchievement)
        {
            CodeRebirthLibPlugin.Logger.LogError($"Trying to complete achievement: {_achievementName} but could not be found. Or it is not a CRInstantAchievement");
            return;
        }

        if (instantAchievement.TryCompleteAchievement())
        {
            _onAchievementCompleted.Invoke();
        }
    }

    public void IncrementAchievement(float amountToIncrement)
    {
        if (!CRMod.AllAchievements().TryGetFromAchievementName(_achievementName, out CRAchievementBaseDefinition? achievementBaseDefinition) || achievementBaseDefinition is not CRProgressiveAchievement progressiveAchievement)
        {
            CodeRebirthLibPlugin.Logger.LogError($"Trying to increment achievement: {_achievementName} but could not be found. Or it is not a CRProgressiveAchievement.");
            return;
        }

        if (progressiveAchievement.IncrementProgress(amountToIncrement))
        {
            _onAchievementCompleted.Invoke();
        }
    }
}