using System.Collections.Generic;

namespace CodeRebirthLib.CRMod;
static class ProgressiveItemHandler
{
    internal static List<ProgressiveItemData> AllProgressiveItems = new();

    internal static void LoadAll(ES3Settings settings)
    {
        foreach (ProgressiveItemData itemData in AllProgressiveItems)
        {
            itemData.Load(settings);
        }
    }

    internal static void SaveAll(ES3Settings settings)
    {
        foreach (ProgressiveItemData itemData in AllProgressiveItems)
        {
            itemData.Save(settings);
        }
    }
}