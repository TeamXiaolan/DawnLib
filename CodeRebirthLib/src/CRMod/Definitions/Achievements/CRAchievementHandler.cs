
using System;

namespace CodeRebirthLib.CRMod;

static class CRAchievementHandler
{
    internal static ES3Settings globalSettings = new($"CRLib.Achievements", ES3.EncryptionType.None);
    public static event Action<CRMAchievementDefinition> OnAchievementUnlocked;

    internal static void LoadAll()
    {
        foreach (CRMAchievementDefinition achievementDefinition in CRModContent.Achievements.Values)
        {
            achievementDefinition.LoadAchievementState(globalSettings);
        }
    }

    internal static void SaveAll()
    {
        foreach (CRMAchievementDefinition achievementDefinition in CRModContent.Achievements.Values)
        {
            achievementDefinition.SaveAchievementState(globalSettings);
        }
    }
}