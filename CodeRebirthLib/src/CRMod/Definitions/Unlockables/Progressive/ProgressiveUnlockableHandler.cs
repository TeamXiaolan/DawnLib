using System.Collections.Generic;

namespace CodeRebirthLib.CRMod;
static class ProgressiveUnlockableHandler
{
    internal static List<ProgressiveUnlockData> AllProgressiveUnlockables = new();

    internal static void LoadAll(ES3Settings settings)
    {
        foreach (ProgressiveUnlockData unlockData in AllProgressiveUnlockables)
        {
            unlockData.Load(settings);
        }
    }

    internal static void SaveAll(ES3Settings settings)
    {
        foreach (ProgressiveUnlockData unlockData in AllProgressiveUnlockables)
        {
            unlockData.Save(settings);
        }
    }
}