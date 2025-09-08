using System.Collections.Generic;
using Dawn.Utils;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Discovery Achievement Definition", menuName = $"{DuskModConstants.Achievements}/Discovery Definition")]
public class DuskDiscoveryAchievement : DuskAchievementDefinition, IProgress
{
    [Tooltip("Unique string ID for each discovery to account for progress.")]
    [field: SerializeField]
    public List<string> UniqueStringIDs { get; private set; }

    public List<string> CurrentlyCollectedUniqueStringIDs { get; private set; } = new List<string>();

    public class DiscoverySaveData(bool completed, List<string> currentlyCollectedUniqueStringIDs) : AchievementSaveData(completed)
    {
        public List<string> CurrentlyCollectedUniqueStringIDs { get; } = currentlyCollectedUniqueStringIDs;
    }

    protected override AchievementSaveData GetSaveData()
    {
        return new DiscoverySaveData(Completed, CurrentlyCollectedUniqueStringIDs);
    }

    protected override void LoadSaveData(AchievementSaveData saveData)
    {
        base.LoadSaveData(saveData);
        CurrentlyCollectedUniqueStringIDs = ((DiscoverySaveData)saveData).CurrentlyCollectedUniqueStringIDs;
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