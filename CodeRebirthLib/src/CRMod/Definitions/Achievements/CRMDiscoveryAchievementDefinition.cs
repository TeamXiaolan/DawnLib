using System.Collections.Generic;
using CodeRebirthLib.Utils;
using UnityEngine;

namespace CodeRebirthLib.CRMod;

[CreateAssetMenu(fileName = "New Discovery Achievement Definition", menuName = "CodeRebirthLib/Definitions/Achievements/Discovery Definition")]
public class CRMDiscoveryAchievement : CRMAchievementDefinition, IProgress
{
    [Tooltip("Unique string ID for each discovery to account for progress.")]
    [field: SerializeField]
    public List<string> UniqueStringIDs { get; private set; }

    public List<string> CurrentlyCollectedUniqueStringIDs { get; private set; } = new List<string>();
    public override void LoadAchievementState(ES3Settings globalSettings)
    {
        base.LoadAchievementState(globalSettings);
        if (Completed)
        {
            CurrentlyCollectedUniqueStringIDs = UniqueStringIDs;
            return;
        }

        CurrentlyCollectedUniqueStringIDs = ES3.Load(Mod.Plugin.GUID + "." + AchievementName + ".CurrentDiscoveryProgress", new List<string>(), globalSettings); // this error'd with expecting [ or null but found 0?
    }

    public override void SaveAchievementState(ES3Settings globalSettings)
    {
        base.SaveAchievementState(globalSettings);
        ES3.Save(Mod.Plugin.GUID + "." + AchievementName + ".CurrentDiscoveryProgress", CurrentlyCollectedUniqueStringIDs, globalSettings);
    }

    public override void ResetProgress()
    {
        CurrentlyCollectedUniqueStringIDs.Clear();
        base.ResetProgress();
    }

    public float MaxProgress => UniqueStringIDs.Count;
    public float CurrentProgress => CurrentlyCollectedUniqueStringIDs.Count;

    public bool TryDiscoverMoreProgress(string UniqueID)
    {
        if (CurrentlyCollectedUniqueStringIDs.Contains(UniqueID))
            return false;

        if (!UniqueStringIDs.Contains(UniqueID))
            return false;

        CurrentlyCollectedUniqueStringIDs.Add(UniqueID);
        if (CurrentProgress >= MaxProgress)
        {
            CurrentlyCollectedUniqueStringIDs = UniqueStringIDs;
            return TryCompleteAchievement();
        }
        return false;
    }

    public bool TryDiscoverMoreProgress(IEnumerable<string> UniqueID)
    {
        foreach (string id in UniqueID)
        {
            if (TryDiscoverMoreProgress(id))
            {
                return true;
            }
        }

        return false;
    }

    public void UndiscoverProgress(string UniqueID)
    {
        if (!UniqueStringIDs.Contains(UniqueID))
            return;

        CurrentlyCollectedUniqueStringIDs.Remove(UniqueID);
    }

    public void UndiscoverProgress(IEnumerable<string> UniqueID)
    {
        foreach (string id in UniqueID)
        {
            UndiscoverProgress(id);
        }
    }
}