
namespace CodeRebirthLib.ContentManagement.Achievements;

static class CRAchievementHandler
{
    internal static ES3Settings globalSettings = new($"CRLib.Achievements", ES3.EncryptionType.None);

    internal static void LoadAll()
    {
        foreach (CRAchievementBaseDefinition achievement in CRMod.AllAchievements())
        {
            achievement.LoadAchievementState(globalSettings);
        }
    }

    internal static void SaveAll()
    {
        foreach (CRAchievementBaseDefinition achievement in CRMod.AllAchievements())
        {
            achievement.SaveAchievementState(globalSettings);
        }
    }
}