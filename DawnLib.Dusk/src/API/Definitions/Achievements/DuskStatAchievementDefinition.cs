using Dawn.Utils;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Stat Achievement Definition", menuName = $"{DuskModConstants.Achievements}/Stat Definition")]
public class DuskStatAchievement : DuskAchievementDefinition, IProgress
{

    [field: SerializeField]
    public float MaxProgress { get; private set; }

    public float CurrentProgress { get; private set; }

    public override void LoadAchievementState(ES3Settings globalSettings)
    {
        base.LoadAchievementState(globalSettings);
        if (Completed)
        {
            CurrentProgress = MaxProgress;
            return;
        }

        CurrentProgress = ES3.Load(Mod.Plugin.GUID + "." + AchievementName + ".CurrentProgress", 0f, globalSettings);
    }

    public override void SaveAchievementState(ES3Settings globalSettings)
    {
        base.SaveAchievementState(globalSettings);
        ES3.Save(Mod.Plugin.GUID + "." + AchievementName + ".CurrentProgress", CurrentProgress, globalSettings);
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
}