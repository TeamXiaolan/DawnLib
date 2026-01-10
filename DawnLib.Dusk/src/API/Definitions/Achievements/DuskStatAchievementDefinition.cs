using Dawn.Utils;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Stat Achievement Definition", menuName = $"{DuskModConstants.Achievements}/Stat Definition")]
public class DuskStatAchievement : DuskAchievementDefinition, IProgress
{
    public class StatSaveData(bool completed, float currentProgress) : AchievementSaveData(completed)
    {
        public float CurrentProgress { get; } = currentProgress;
    }

    [field: SerializeField]
    public float MaxProgress { get; private set; }

    public float CurrentProgress { get; private set; }

    protected override AchievementSaveData GetSaveData()
    {
        return new StatSaveData(Completed, CurrentProgress);
    }

    protected override void LoadSaveData(AchievementSaveData saveData)
    {
        base.LoadSaveData(saveData);
        CurrentProgress = ((StatSaveData)saveData).CurrentProgress;
    }

    public override void ResetProgress()
    {
        CurrentProgress = 0f;
        base.ResetProgress();
    }

    public bool IncrementProgress(float amount)
    {
        CurrentProgress = Mathf.Max(0f, CurrentProgress + amount);
        if (CurrentProgress >= MaxProgress)
        {
            CurrentProgress = MaxProgress;
            return TryCompleteAchievement();
        }
        return false;
    }

    public void DecrementProgress(float amount)
    {
        CurrentProgress -= Mathf.Max(0f, amount);
        if (CurrentProgress <= 0f)
        {
            CurrentProgress = 0f;
        }
    }

    public override void TryNetworkRegisterAssets() { }
}