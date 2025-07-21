using UnityEngine;

namespace CodeRebirthLib.ContentManagement.Achievements;

[CreateAssetMenu(fileName = "New Progressive Achievement Definition", menuName = "CodeRebirthLib/Definitions/Achievements/Progressive Definition")]
public class CRProgressiveAchievement : CRAchievementBaseDefinition
{
    [field: SerializeField]
    public float MaxProgress { get; private set; }

    public float CurrentProgress { get; private set; }

    public float Percentage => CurrentProgress / MaxProgress;

    public override void LoadAchievementState(ES3Settings globalSettings)
    {
        base.LoadAchievementState(globalSettings);
        if (Completed)
        {
            CurrentProgress = MaxProgress;
            return;
        }

        CurrentProgress = ES3.Load(_mod.Plugin.GUID + "." + AchievementName + ".CurrentProgress", 0f, globalSettings);
    }

    public override void SaveAchievementState(ES3Settings globalSettings)
    {
        base.SaveAchievementState(globalSettings);
        ES3.Save(_mod.Plugin.GUID + "." + AchievementName + ".CurrentProgress", CurrentProgress, globalSettings);
    }

    public bool IncrementProgress(float amount)
    {
        CurrentProgress += amount;
        if (CurrentProgress >= MaxProgress)
        {
            CurrentProgress = MaxProgress;
            return TryCompleteAchievement();
        }
        return false;
    }
}