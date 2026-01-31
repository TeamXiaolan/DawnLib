using System;
using System.Collections.Generic;
using Dawn.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Dawn.Internal;
public static class UnlockableSaveDataHandler
{
    internal static List<PlaceableShipObject> placeableShipObjects = new();

    private static readonly NamespacedKey _namespacedKey = NamespacedKey.From("dawn_lib", "ship_unlockables_save_data");

    internal static void LoadSavedUnlockables(PersistentDataContainer dataContainer)
    {
        JObject? root = TryGetNewFormat(dataContainer);

        if (root == null)
        {
            root = TryMigrateLegacyFormat(dataContainer);

            if (root == null)
            {
                return;
            }
        }

        JArray keysArray = root["keys"] as JArray ?? [];
        JArray itemsArray = root["items"] as JArray ?? [];

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
            if (string.IsNullOrWhiteSpace(keyString) || !NamespacedKey.TryParse(keyString, out NamespacedKey? unlockableKey))
            {
                DawnPlugin.Logger.LogWarning($"Invalid unlockable key '{keyString}' in save; skipping.");
                continue;
            }

            if (!LethalContent.Unlockables.TryGetValue(unlockableKey, out DawnUnlockableItemInfo unlockableItemInfo))
            {
                DawnPlugin.Logger.LogWarning($"Unlockable: {unlockableKey} doesn't exist anymore (mod removed?); skipping.");
                continue;
            }

            // 1–3: position
            float posX = row[1].ToObject<float>();
            float posY = row[2].ToObject<float>();
            float posZ = row[3].ToObject<float>();
            Vector3 savedPos = new(posX, posY, posZ);

            // 4–6: rotation
            float rotX = row[4].ToObject<float>();
            float rotY = row[5].ToObject<float>();
            float rotZ = row[6].ToObject<float>();
            Vector3 savedRot = new(rotX, rotY, rotZ);

            // 7–9: flags
            bool inStorage = row[7].ToObject<bool>();
            bool hasBeenMoved = row[8].ToObject<bool>();
            bool placedAtQuotaStart = row[9].ToObject<bool>();

            // 10: savedData (currently unused / TODO)
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

                // TODO: apply savedData

                if (inStorage)
                {
                    if (hasBeenMoved && !unlockableItem.spawnPrefab)
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
            if (!StartOfRoundRefs.Instance.unlockablesList.unlockables[placeableShipObject.unlockableID].spawnPrefab &&
                StartOfRoundRefs.Instance.unlockablesList.unlockables[placeableShipObject.unlockableID].inStorage)
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
        List<string> keysList = new();
        Dictionary<string, int> keyIndexLookup = new();

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

    private static JObject? TryGetNewFormat(PersistentDataContainer dataContainer)
    {
        JObject root;

        try
        {
            root = dataContainer.GetOrCreateDefault<JObject>(_namespacedKey);
        }
        catch (Exception e)
        {
            DawnPlugin.Logger.LogWarning($"Failed to read ship unlockables as new-format JObject; " + $"will try legacy migration. ({e.GetType().Name}: {e.Message})");
            return null;
        }

        if (root == null || root.Type != JTokenType.Object)
        {
            return null;
        }

        JToken? keysToken = root["keys"];
        JToken? itemsToken = root["items"];

        if (keysToken == null && itemsToken == null)
        {
            root["keys"] = new JArray();
            root["items"] = new JArray();
            return root;
        }

        if (keysToken is null)
        {
            root["keys"] = new JArray();
        }

        if (itemsToken is null)
        {
            root["items"] = new JArray();
        }

        if (root["keys"] is not JArray || root["items"] is not JArray)
        {
            return null;
        }

        return root;
    }

    public struct UnlockableSaveData(NamespacedKey unlockableNamespacedKey, Vector3 savePosition, Vector3 saveRotation, bool inStorage, bool hasBeenMoved, bool placedAtQuotaStart, JToken savedData)
    {
        public NamespacedKey UnlockableNamespacedKey = unlockableNamespacedKey;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 SavedSpawnPosition = savePosition;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 SavedSpawnRotation = saveRotation;

        public bool InStorage = inStorage;
        public bool HasBeenMoved = hasBeenMoved;
        public bool PlacedAtQuotaStart = placedAtQuotaStart;
        public JToken SavedData = savedData;
    }

    private static JObject? TryMigrateLegacyFormat(PersistentDataContainer dataContainer)
    {
        List<UnlockableSaveData> legacyList;

        try
        {
            legacyList = dataContainer.GetOrCreateDefault<List<UnlockableSaveData>>(_namespacedKey);
        }
        catch (Exception e)
        {
            DawnPlugin.Logger.LogWarning($"Failed to read legacy ship unlockables for migration: " + $"{e.GetType().Name}: {e.Message}");
            return null;
        }

        if (legacyList == null || legacyList.Count == 0)
            return null;

        DawnPlugin.Logger.LogInfo($"Migrating {legacyList.Count} legacy ship unlockables to new compact format.");

        List<string> keysList = new();
        Dictionary<NamespacedKey, int> keyIndexLookup = new();
        JArray itemsArray = new();

        foreach (UnlockableSaveData legacy in legacyList)
        {
            NamespacedKey key = legacy.UnlockableNamespacedKey;

            if (!keyIndexLookup.TryGetValue(key, out int keyIndex))
            {
                keyIndex = keysList.Count;
                keysList.Add(key.ToString());
                keyIndexLookup[key] = keyIndex;
            }

            JToken savedData = legacy.SavedData ?? 0;
            JArray row = new()
            {
                keyIndex,                       // 0
                legacy.SavedSpawnPosition.x,    // 1
                legacy.SavedSpawnPosition.y,    // 2
                legacy.SavedSpawnPosition.z,    // 3
                legacy.SavedSpawnRotation.x,    // 4
                legacy.SavedSpawnRotation.y,    // 5
                legacy.SavedSpawnRotation.z,    // 6
                legacy.InStorage,               // 7
                legacy.HasBeenMoved,            // 8
                legacy.PlacedAtQuotaStart,      // 9
                savedData                       // 10
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

        DawnPlugin.Logger.LogInfo($"Migration complete. Migrated {itemsArray.Count} unlockables into new compact format.");

        return root;
    }
}