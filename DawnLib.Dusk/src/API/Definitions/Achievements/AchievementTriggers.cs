using System.Collections.Generic;
using Dawn.Internal;
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
        Debuggers.Achievements?.Log($"Trying to complete achievement: {_reference.TypedKey}");
        if (!_reference.TryResolve(out DuskAchievementDefinition achievementDefinition))
            return;

        if (DuskModContent.Achievements.TryTriggerAchievement(achievementDefinition.TypedKey))
        {
            _onAchievementCompleted.Invoke();
        }
    }

    public void TryIncrementAchievement(float amountToIncrement)
    {
        Debuggers.Achievements?.Log($"Trying to increment achievement: {_reference.TypedKey} by {amountToIncrement}");
        if (!_reference.TryResolve(out DuskAchievementDefinition achievementDefinition))
            return;

        if (DuskModContent.Achievements.TryIncrementAchievement(achievementDefinition.TypedKey, amountToIncrement))
        {
            _onAchievementCompleted.Invoke();
        }
    }

    public void TryDiscoverMoreProgressAchievement(string uniqueStringID)
    {
        Debuggers.Achievements?.Log($"Trying to discover more progress for achievement: {_reference.TypedKey} with unique string id: {uniqueStringID}");
        if (!_reference.TryResolve(out DuskAchievementDefinition achievementDefinition))
            return;

        if (DuskModContent.Achievements.TryDiscoverMoreProgressAchievement(achievementDefinition.TypedKey, uniqueStringID))
        {
            _onAchievementCompleted.Invoke();
        }
    }

    public void TryDiscoverMoreProgressAchievement(List<string> uniqueStringIDs)
    {
        Debuggers.Achievements?.Log($"Trying to discover more progress for achievement: {_reference.TypedKey} with unique string ids: {string.Join(", ", uniqueStringIDs)}");
        if (!_reference.TryResolve(out DuskAchievementDefinition achievementDefinition))
            return;

        if (DuskModContent.Achievements.TryDiscoverMoreProgressAchievement(achievementDefinition.TypedKey, uniqueStringIDs))
        {
            _onAchievementCompleted.Invoke();
        }
    }

    public void ResetAllAchievementProgress()
    {
        Debuggers.Achievements?.Log($"Trying to reset all progress for achievement: {_reference.TypedKey}");
        if (!_reference.TryResolve(out DuskAchievementDefinition achievementDefinition))
            return;

        achievementDefinition.ResetProgress();
    }

    public void SoftResetAllAchievementProgress()
    {
        Debuggers.Achievements?.Log($"Trying to soft reset all progress for achievement: {_reference.TypedKey}");
        if (!_reference.TryResolve(out DuskAchievementDefinition achievementDefinition))
            return;

        achievementDefinition.SoftResetProgress();
    }
}
