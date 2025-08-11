using UnityEngine;
using UnityEngine.Events;

namespace CodeRebirthLib;

public class AchievementTriggers : MonoBehaviour
{
    [SerializeField]
    private NamespacedKey<CRAchievementInfo> _namedSpacedKey = default!;

    [SerializeField]
    private UnityEvent _onAchievementCompleted = new UnityEvent();

    public void TryCompleteAchievement()
    {
        if (CRLibContent.Achievements[_namedSpacedKey].TryTriggerAchievement(_achievementReference))
        {
            _onAchievementCompleted.Invoke();
        }
    }

    public void TryIncrementAchievement(float amountToIncrement)
    {
        if (CRLibContent.Achievements[_namedSpacedKey].TryIncrementAchievement(_achievementReference, amountToIncrement))
        {
            _onAchievementCompleted.Invoke();
        }
    }

    public void TryDiscoverMoreProgressAchievement(string uniqueStringID)
    {
        if (CRLibContent.Achievements[_namedSpacedKey].TryDiscoverMoreProgressAchievement(_achievementReference, uniqueStringID))
        {
            _onAchievementCompleted.Invoke();
        }
    }

    public void ResetAllAchievementProgress()
    {
        CRLibContent.Achievements[_namedSpacedKey].ResetAchievementProgress(_achievementReference);
    }
}