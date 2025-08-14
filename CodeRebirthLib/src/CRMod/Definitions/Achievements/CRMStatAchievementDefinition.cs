using CodeRebirthLib.Utils;
using UnityEngine;

namespace CodeRebirthLib.CRMod;

[CreateAssetMenu(fileName = "New Stat Achievement Definition", menuName = "CodeRebirthLib/Definitions/Achievements/Stat Definition")]
public class CRMStatAchievement : CRMAchievementDefinition, IProgress
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
        base.ResetProgress();
        CurrentProgress = 0f;
        SaveAchievementState(CRAchievementHandler.globalSettings);
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