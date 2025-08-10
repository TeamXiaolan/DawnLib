using UnityEngine;
using UnityEngine.Events;

namespace CodeRebirthLib;

public class AchievementTriggers : MonoBehaviour
{
    [SerializeReference]
    private CRAchievementReference _achievementReference = new("");

    [SerializeField]
    private UnityEvent _onAchievementCompleted = new UnityEvent();

    public void TryCompleteAchievement()
    {
        if (CRMod.AllAchievements().TryTriggerAchievement(_achievementReference))
        {
            _onAchievementCompleted.Invoke();
        }
    }

    public void TryIncrementAchievement(float amountToIncrement)
    {
        if (CRMod.AllAchievements().TryIncrementAchievement(_achievementReference, amountToIncrement))
        {
            _onAchievementCompleted.Invoke();
        }
    }

    public void TryDiscoverMoreProgressAchievement(string uniqueStringID)
    {
        if (CRMod.AllAchievements().TryDiscoverMoreProgressAchievement(_achievementReference, uniqueStringID))
        {
            _onAchievementCompleted.Invoke();
        }
    }

    public void ResetAllAchievementProgress()
    {
        CRMod.AllAchievements().ResetAchievementProgress(_achievementReference);
    }
}