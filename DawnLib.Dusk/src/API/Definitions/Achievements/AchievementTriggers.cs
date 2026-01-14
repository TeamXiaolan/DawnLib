using UnityEngine;
using UnityEngine.Events;

namespace Dusk;

[AddComponentMenu($"{DuskModConstants.MenuName}/Achievements/Achivement Triggers")]
public class AchievementTriggers : MonoBehaviour
{
    [SerializeReference]
    private DuskAchievementReference _reference = default!;

    [SerializeField]
    private UnityEvent _onAchievementCompleted = new UnityEvent();

    public void TryCompleteAchievement()
    {
        if (!_reference.TryResolve(out DuskAchievementDefinition achievementDefinition))
            return;

        if (DuskModContent.Achievements.TryTriggerAchievement(achievementDefinition.TypedKey))
        {
            _onAchievementCompleted.Invoke();
        }
    }

    public void TryIncrementAchievement(float amountToIncrement)
    {
        if (!_reference.TryResolve(out DuskAchievementDefinition achievementDefinition))
            return;

        if (DuskModContent.Achievements.TryIncrementAchievement(achievementDefinition.TypedKey, amountToIncrement))
        {
            _onAchievementCompleted.Invoke();
        }
    }

    public void TryDiscoverMoreProgressAchievement(string uniqueStringID)
    {
        if (!_reference.TryResolve(out DuskAchievementDefinition achievementDefinition))
            return;

        if (DuskModContent.Achievements.TryDiscoverMoreProgressAchievement(achievementDefinition.TypedKey, uniqueStringID))
        {
            _onAchievementCompleted.Invoke();
        }
    }

    public void ResetAllAchievementProgress()
    {
        if (!_reference.TryResolve(out DuskAchievementDefinition achievementDefinition))
            return;

        achievementDefinition.ResetProgress();
    }
}
