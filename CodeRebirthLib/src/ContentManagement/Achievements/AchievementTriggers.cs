using UnityEngine;
using UnityEngine.Events;

namespace CodeRebirthLib.ContentManagement.Achievements;

public class AchievementTriggers : MonoBehaviour
{
    [SerializeField]
    private string _achievementName = string.Empty;

    [SerializeField]
    private UnityEvent _onAchievementCompleted = new UnityEvent();

    public void TryCompleteAchievement()
    {
        if (CRMod.AllAchievements().TryTriggerAchievement(_achievementName))
        {
            _onAchievementCompleted.Invoke();
        }
    }

    public void TryIncrementAchievement(float amountToIncrement)
    {
        if (CRMod.AllAchievements().TryIncrementAchievement(_achievementName, amountToIncrement))
        {
            _onAchievementCompleted.Invoke();
        }
    }

    public void TryDiscoverMoreProgressAchievement(string uniqueStringID)
    {
        if (CRMod.AllAchievements().TryDiscoverMoreProgressAchievement(_achievementName, uniqueStringID))
        {
            _onAchievementCompleted.Invoke();
        }
    }
}