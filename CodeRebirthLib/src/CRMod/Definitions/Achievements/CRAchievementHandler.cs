
using System;

namespace CodeRebirthLib;

static class CRAchievementHandler
{
    internal static ES3Settings globalSettings = new($"CRLib.Achievements", ES3.EncryptionType.None);
    public static event Action<CRAchievementDefinition> OnAchievementUnlocked;

    internal static void LoadAll()
    {
        foreach (CRAchievementInfo achievementInfo in CRLibContent.Achievements.Values)
        {
            achievementInfo.LoadAchievementState(globalSettings);
        }
    }

    internal static void SaveAll()
    {
        foreach (CRAchievementInfo achievementInfo in CRLibContent.Achievements.Values)
        {
            achievementInfo.SaveAchievementState(globalSettings);
        }
    }
}