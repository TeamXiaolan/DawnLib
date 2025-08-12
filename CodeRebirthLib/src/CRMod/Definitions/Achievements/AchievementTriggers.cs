using UnityEngine;
using UnityEngine.Events;

namespace CodeRebirthLib.CRMod;

public class AchievementTriggers : MonoBehaviour
{
    [SerializeField]
    private CRContentReference<CRAchievementDefinition, CRAchievementInfo> _reference = default!;

    [SerializeField]
    private UnityEvent _onAchievementCompleted = new UnityEvent();

    public void TryCompleteAchievement()
    {
        if (_reference.TryResolve(out CRAchievementInfo info))
        {
            // todo: something with CRAchievementInfo
            _onAchievementCompleted.Invoke();
        }
    }

    public void TryIncrementAchievement(float amountToIncrement)
    {
        if (_reference.TryResolve(out CRAchievementInfo info))
        {
            // todo: something with CRAchievementInfo
            _onAchievementCompleted.Invoke();
        }
    }

    public void TryDiscoverMoreProgressAchievement(string uniqueStringID)
    {
        if (_reference.TryResolve(out CRAchievementInfo info))
        {
            // todo: something with CRAchievementInfo
            _onAchievementCompleted.Invoke();
        }
    }

    public void ResetAllAchievementProgress()
    {
        
    }
}