using System.Collections.Generic;
using Dawn.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Dawn.Internal;
public static class UnlockableSaveDataHandler
{
    internal static List<PlaceableShipObject> placeableShipObjects = new();
    private static NamespacedKey _namespacedKey = NamespacedKey.From("dawn_lib", "ship_unlockables_save_data");

    internal static void LoadSavedUnlockables(PersistentDataContainer dataContainer)
    {
        JObject root = dataContainer.GetOrCreateDefault<JObject>(_namespacedKey) ?? new JObject();

        JArray keysArray = root["keys"] as JArray ?? new JArray();
        JArray itemsArray = root["items"] as JArray ?? new JArray();

        List<int> PlacedAtStartOfQuotas = new();

        foreach (JToken token in itemsArray)
        {
            if (token is not JArray row || row.Count < 10)
            {
                DawnPlugin.Logger.LogWarning("Malformed unlockable row in save data; skipping.");
                continue;
            }

            int keyIndex = row[0]!.ToObject<int>();
            if (keyIndex < 0 || keyIndex >= keysArray.Count)
            {
                DawnPlugin.Logger.LogWarning($"Invalid unlockable key index {keyIndex} in save; skipping.");
                continue;
            }

            string? keyString = keysArray[keyIndex].ToObject<string>();
            if (string.IsNullOrEmpty(keyString) || !NamespacedKey.TryParse(keyString, out NamespacedKey? unlockableKey))
            {
                DawnPlugin.Logger.LogWarning($"Invalid unlockable key '{keyString}' in save; skipping.");
                continue;
            }

            if (!LethalContent.Unlockables.TryGetValue(unlockableKey, out DawnUnlockableItemInfo unlockableItemInfo))
            {
                DawnPlugin.Logger.LogWarning(
                    $"Unlockable: {unlockableKey} doesn't exist anymore (mod removed?); skipping.");
                continue;
            }

            // 1–3: position
            float px = row[1].ToObject<float>();
            float py = row[2].ToObject<float>();
            float pz = row[3].ToObject<float>();
            Vector3 savedPos = new(px, py, pz);

            // 4–6: rotation
            float rx = row[4].ToObject<float>();
            float ry = row[5].ToObject<float>();
            float rz = row[6].ToObject<float>();
            Vector3 savedRot = new(rx, ry, rz);

            // 7–9: flags
            bool inStorage = row[7].ToObject<bool>();
            bool hasBeenMoved = row[8].ToObject<bool>();
            bool placedAtQuotaStart = row[9].ToObject<bool>();

            // 10: savedData
            JToken savedData = row.Count > 10 ? row[10] : 0;

            Debuggers.SaveManager?.Log($"Loading unlockable: {unlockableKey} from save data with information: " + $"{savedPos}, {savedRot}, {inStorage}, {hasBeenMoved}, {placedAtQuotaStart}.");

            UnlockableItem unlockableItem = unlockableItemInfo.UnlockableItem;
            int indexInList = StartOfRound.Instance.unlockablesList.unlockables.IndexOf(unlockableItem);

            if (placedAtQuotaStart)
            {
                PlacedAtStartOfQuotas.Add(indexInList);
            }

            if (!unlockableItem.alreadyUnlocked || unlockableItem.IsPlaceable)
            {
                unlockableItem.hasBeenUnlockedByPlayer = !unlockableItem.alreadyUnlocked;

                unlockableItem.placedPosition = savedPos;
                unlockableItem.placedRotation = savedRot;
                unlockableItem.inStorage = inStorage;
                unlockableItem.hasBeenMoved = hasBeenMoved;


                if (inStorage)
                {
                    if (hasBeenMoved && !unlockableItem.spawnPrefab)
                    {
                        foreach (PlaceableShipObject placeableShipObject in placeableShipObjects)
                        {
                            if (placeableShipObject.unlockableID == indexInList)
                            {
                                ShipBuildModeManager.Instance.PlaceShipObject(unlockableItem.placedPosition, unlockableItem.placedRotation, placeableShipObject, false
                                );
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
        // keys[i] = "mod_namespace:unlockable_name"
        List<string> keysList = new();
        Dictionary<string, int> keyIndexLookup = new();

        // items = [ [ keyIndex, px, py, pz, rx, ry, rz, inStorage, hasBeenMoved, placedAtQuotaStart, savedData ], ... ]
        JArray itemsArray = new();

        for (int i = 0; i < StartOfRound.Instance.unlockablesList.unlockables.Count; i++)
        {
            UnlockableItem unlockableData = StartOfRound.Instance.unlockablesList.unlockables[i];

            if (unlockableData.unlockableType == 753)
            {
                Debuggers.SaveManager?.Log($"Skipping saving unlockable: {unlockableData.unlockableName} into save data, " + "this is probably moresuits adding a bajillion extra orange suit duplicates.");
                continue;
            }

            Debuggers.SaveManager?.Log($"Checking whether to save unlockable: {unlockableData.unlockableName} into save data.");

            if (!unlockableData.hasBeenUnlockedByPlayer && !unlockableData.alreadyUnlocked)
            {
                continue;
            }

            DawnUnlockableItemInfo? unlockableInfo = unlockableData.GetDawnInfo();
            if (unlockableInfo == null)
            {
                DawnPlugin.Logger.LogError($"Unlockable: {unlockableData.unlockableName} doesn't have a DawnUnlockableInfo, " + "this means this unlockable cannot be committed to the savefile, " + "contact the developer of this unlockable to fix this ASAP!");
                continue;
            }

            string keyString = unlockableInfo.Key.ToString();
            if (!keyIndexLookup.TryGetValue(keyString, out int keyIndex))
            {
                keyIndex = keysList.Count;
                keysList.Add(keyString);
                keyIndexLookup[keyString] = keyIndex;
            }

            Debuggers.SaveManager?.Log($"Saving unlockable: {unlockableInfo.Key} into save data.");
            bool placedAtQuotaStart = TimeOfDayRefs.Instance.furniturePlacedAtQuotaStart.Contains(i);

            if (!unlockableData.alreadyUnlocked && !unlockableData.inStorage)
            {
                GameObject unlockableGameObject = StartOfRound.Instance.SpawnedShipUnlockables[i].gameObject;
                Debuggers.SaveManager?.Log($"Unlockable: {unlockableInfo.TypedKey} has GameObject: {unlockableGameObject}.");
            }

            // TODO
            JToken savedData = 0;

            JArray row = new()
            {
                keyIndex,                             // 0
                unlockableData.placedPosition.x,      // 1
                unlockableData.placedPosition.y,      // 2
                unlockableData.placedPosition.z,      // 3
                unlockableData.placedRotation.x,      // 4
                unlockableData.placedRotation.y,      // 5
                unlockableData.placedRotation.z,      // 6
                unlockableData.inStorage,             // 7
                unlockableData.hasBeenMoved,          // 8
                placedAtQuotaStart,                   // 9
                savedData                             // 10
            };

            itemsArray.Add(row);
        }

        JObject root = new()
        {
            ["keys"] = new JArray(keysList),
            ["items"] = itemsArray
        };

        using (dataContainer.CreateEditContext())
        {
            dataContainer.Set(_namespacedKey, root);
        }
    }
}