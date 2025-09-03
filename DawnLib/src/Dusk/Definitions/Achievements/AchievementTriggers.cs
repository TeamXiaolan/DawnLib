using UnityEngine;
using UnityEngine.Events;

namespace Dawn.Dusk;

public class AchievementTriggers : MonoBehaviour
{
    [SerializeField]
    private DuskAchievementReference _reference = default!;

    [SerializeField]
    private UnityEvent _onAchievementCompleted = new UnityEvent();

    public void TryCompleteAchievement()
    {
        if (DuskModContent.Achievements.TryTriggerAchievement(_reference.Resolve().TypedKey))
        {
            _onAchievementCompleted.Invoke();
        }
    }

    public void TryIncrementAchievement(float amountToIncrement)
    {
        if (DuskModContent.Achievements.TryIncrementAchievement(_reference.Resolve().TypedKey, amountToIncrement))
        {
            _onAchievementCompleted.Invoke();
        }
    }

    public void TryDiscoverMoreProgressAchievement(string uniqueStringID)
    {
        if (DuskModContent.Achievements.TryDiscoverMoreProgressAchievement(_reference.Resolve().TypedKey, uniqueStringID))
        {
            _onAchievementCompleted.Invoke();
        }
    }

    public void ResetAllAchievementProgress()
    {

    }
}