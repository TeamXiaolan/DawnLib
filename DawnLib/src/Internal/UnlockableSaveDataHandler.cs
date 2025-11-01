using System.Collections.Generic;
using Dawn.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Dawn.Internal;
public static class UnlockableSaveDataHandler
{
    internal static List<PlaceableShipObject> placeableShipObjects = new();
    private static NamespacedKey _namespacedKey = NamespacedKey.From("dawn_lib", "ship_unlockables_save_data");

    internal static void LoadSavedUnlockables(PersistentDataContainer dataContainer)
    {
        // TODO: replace a lot of the things here with the new component when i make it
        List<UnlockableSaveData> UnlockableSaveDataList = dataContainer.GetOrCreateDefault<List<UnlockableSaveData>>(_namespacedKey);
        List<int> PlacedAtStartOfQuotas = new();
        foreach (UnlockableSaveData unlockableData in UnlockableSaveDataList)
        {
            Debuggers.SaveManager?.Log($"Loading unlockable: {unlockableData.UnlockableNamespacedKey} from save data with information: {unlockableData.SavedSpawnPosition}, {unlockableData.SavedSpawnRotation}, {unlockableData.InStorage}, {unlockableData.HasBeenMoved}, {unlockableData.PlacedAtQuotaStart}.");
            if (!LethalContent.Unlockables.TryGetValue(unlockableData.UnlockableNamespacedKey, out DawnUnlockableItemInfo unlockableItemInfo))
            {
                DawnPlugin.Logger.LogWarning($"Unlockable: {unlockableData.UnlockableNamespacedKey} doesn't exist in the game, this means this item cannot be loaded from the savefile, presumably you removed a mod that added this time previously.");
                continue;
            }

            UnlockableItem unlockableItem = unlockableItemInfo.UnlockableItem;
            int indexInList = StartOfRound.Instance.unlockablesList.unlockables.IndexOf(unlockableItemInfo.UnlockableItem);
            if (unlockableData.PlacedAtQuotaStart)
            {
                PlacedAtStartOfQuotas.Add(unlockableItem.shopSelectionNode.shipUnlockableID);
            }

            if (!unlockableItem.alreadyUnlocked || unlockableItem.IsPlaceable)
            {
                unlockableItem.hasBeenUnlockedByPlayer = !unlockableItem.alreadyUnlocked;

                unlockableItem.placedPosition = unlockableData.SavedSpawnPosition;
                unlockableItem.placedRotation = unlockableData.SavedSpawnRotation;
                unlockableItem.inStorage = unlockableData.InStorage;
                unlockableItem.hasBeenMoved = unlockableData.HasBeenMoved;
                if (unlockableData.InStorage)
                {
                    if (unlockableData.HasBeenMoved && !unlockableItem.spawnPrefab)
                    {
                        foreach (PlaceableShipObject placeableShipObject in placeableShipObjects)
                        {
                            if (placeableShipObject.unlockableID == indexInList)
                            {
                                ShipBuildModeManager.Instance.PlaceShipObject(unlockableItem.placedPosition, unlockableItem.placedRotation, placeableShipObject, false);
                            }
                        }                        
                    }
                }
                else if (!StartOfRoundRefs.Instance.SpawnedShipUnlockables.ContainsKey(indexInList))
                {
                    StartOfRound.Instance.SpawnUnlockable(indexInList, false);
                }
            }
        }

        if (PlacedAtStartOfQuotas.Count > 0)
        {
            TimeOfDayRefs.Instance.furniturePlacedAtQuotaStart = PlacedAtStartOfQuotas;
            TimeOfDayRefs.Instance.CalculateLuckValue();
        }

        foreach (PlaceableShipObject placeableShipObject in placeableShipObjects)
        {
            if (!StartOfRoundRefs.Instance.unlockablesList.unlockables[placeableShipObject.unlockableID].spawnPrefab && StartOfRoundRefs.Instance.unlockablesList.unlockables[placeableShipObject.unlockableID].inStorage)
            {
                placeableShipObject.parentObject.disableObject = true;
            }
        }

        for (int i = 0; i < StartOfRoundRefs.Instance.unlockablesList.unlockables.Count; i++)
        {
            if ((i != 0 || !StartOfRoundRefs.Instance.isChallengeFile) && (StartOfRoundRefs.Instance.unlockablesList.unlockables[i].alreadyUnlocked || (StartOfRoundRefs.Instance.unlockablesList.unlockables[i].unlockedInChallengeFile && StartOfRoundRefs.Instance.isChallengeFile)) && !StartOfRoundRefs.Instance.unlockablesList.unlockables[i].IsPlaceable)
            {
                StartOfRoundRefs.Instance.SpawnUnlockable(i, false);
            }
        }

        foreach (PlaceableShipObject placeableShipObject in placeableShipObjects)
        {
            placeableShipObject.parentObject.NetworkObject.OnSpawn(() =>
            {
                if (!placeableShipObject.parentObject.NetworkObject.TrySetParent(StartOfRoundRefs.Instance.shipAnimatorObject, true))
                {
                    DawnPlugin.Logger.LogError($"Parenting of object: {placeableShipObject.parentObject.gameObject.name} failed.");
                }
            });
            placeableShipObject.parentObject.MoveToOffset();
        }
        Physics.SyncTransforms();
    }

    internal static void SaveAllUnlockables(PersistentDataContainer dataContainer)
    {
        List<UnlockableSaveData> allShipUnlockablesData = new();
        for (int i = 0; i < StartOfRound.Instance.unlockablesList.unlockables.Count; i++)
        {
            UnlockableItem unlockableData = StartOfRound.Instance.unlockablesList.unlockables[i];
            Debuggers.SaveManager?.Log($"Checking whether to save unlockable: {unlockableData.unlockableName} into save data.");
            if (!unlockableData.hasBeenUnlockedByPlayer && !unlockableData.alreadyUnlocked)
            {
                continue;
            }

            DawnUnlockableItemInfo? unlockableInfo = unlockableData.GetDawnInfo();
            if (unlockableInfo == null)
            {
                DawnPlugin.Logger.LogError($"Unlockable: {unlockableData.unlockableName} doesn't have a DawnUnlockableInfo, this means this unlockable cannot be committed to the savefile, contact the developer of this unlockable to fix this ASAP!");
                continue;
            }
            Debuggers.SaveManager?.Log($"Saving unlockable: {unlockableInfo.Key} into save data.");
            bool placedAtQuotaStart = false;
            if (unlockableData.shopSelectionNode != null)
            {
                placedAtQuotaStart = TimeOfDayRefs.Instance.furniturePlacedAtQuotaStart.Contains(unlockableData.shopSelectionNode.shipUnlockableID);
            }
            if (!unlockableData.alreadyUnlocked)
            {
                GameObject unlockableGameObject = StartOfRound.Instance.SpawnedShipUnlockables[i].gameObject;
                Debuggers.SaveManager?.Log($"Unlockable: {unlockableInfo.TypedKey} has GameObject: {unlockableGameObject}.");
            }

            // TODO: replace the 0 with the proper value by saving all instances of the new unlockable component that i need to make
            allShipUnlockablesData.Add(new UnlockableSaveData(unlockableInfo.Key, unlockableData.placedPosition, unlockableData.placedRotation, unlockableData.inStorage, unlockableData.hasBeenMoved, placedAtQuotaStart, 0));
        }

        using (dataContainer.CreateEditContext())
        {
            dataContainer.Set(_namespacedKey, allShipUnlockablesData);
        }
    }

    public struct UnlockableSaveData(NamespacedKey unlockableNamespacedKey, Vector3 savePosition, Vector3 saveRotation, bool inStorage, bool hasBeenMoved, bool PlacedAtQuotaStart, JToken jToken)
    {
        public NamespacedKey UnlockableNamespacedKey = unlockableNamespacedKey;
        [JsonConverter(typeof(Vector3Converter))] public Vector3 SavedSpawnPosition = savePosition;
        [JsonConverter(typeof(Vector3Converter))] public Vector3 SavedSpawnRotation = saveRotation;
        public bool InStorage = inStorage;
        public bool HasBeenMoved = hasBeenMoved;
        public bool PlacedAtQuotaStart = PlacedAtQuotaStart;
        public JToken SavedData = jToken;
    }
}