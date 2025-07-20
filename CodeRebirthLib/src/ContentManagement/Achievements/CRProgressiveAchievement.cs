using UnityEngine;

namespace CodeRebirthLib.ContentManagement.Achievements;

[CreateAssetMenu(fileName = "New Progressive Achievement Definition", menuName = "CodeRebirthLib/Definitions/Progressive Achievement Definition")]
public class CRProgressiveAchievement : CRAchievementBaseDefinition
{
    [field: SerializeField]
    public int MaxProgress { get; private set; }

    public int CurrentProgress { get; private set; }

    public override void LoadAchievementState(ES3Settings globalSettings)
    {
        base.LoadAchievementState(globalSettings);
        if (Completed)
        {
            CurrentProgress = MaxProgress;
            return;
        }

        CurrentProgress = ES3.Load(_mod.Plugin.GUID + "." + AchievementName + ".CurrentProgress", 0, globalSettings);
    }

    public override void SaveAchievementState(ES3Settings globalSettings)
    {
        base.SaveAchievementState(globalSettings);
        ES3.Save(_mod.Plugin.GUID + "." + AchievementName + ".CurrentProgress", CurrentProgress, globalSettings);
    }
}