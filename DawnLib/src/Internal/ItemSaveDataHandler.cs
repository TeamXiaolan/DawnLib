using System;
using System.Collections;
using System.Collections.Generic;
using Dawn.Interfaces;
using Dawn.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Dawn.Internal;

public static class ItemSaveDataHandler
{
    private static readonly List<Collider> _validItemSpawningColliders = new();
    public static IReadOnlyList<Collider> ValidItemSpawningColliders => _validItemSpawningColliders;
    internal static GrabbableObject[] AllShipItems = [];

    public static void AddColliderForItemSaving(Collider collider)
    {
        _validItemSpawningColliders.Add(collider);
    }

    private static void ValidateColliderEntries()
    {
        for (int i = _validItemSpawningColliders.Count - 1; i >= 0; i--)
        {
            if (_validItemSpawningColliders[i] == null)
            {
                _validItemSpawningColliders.RemoveAt(i);
            }
        }
    }

    internal static void LoadSavedItems(PersistentDataContainer dataContainer)
    {
        JObject? root = TryGetCurrentFormat(dataContainer);
        if (root == null)
        {
            return;
        }

        LoadFromRoot(root);
    }

    internal static void SaveAllItems(PersistentDataContainer dataContainer)
    {
        AllShipItems = GameObject.FindObjectsOfType<GrabbableObject>();

        List<string> keysList = new();
        Dictionary<string, int> keyIndexLookup = new();
        JArray itemsArray = new();

        foreach (GrabbableObject item in AllShipItems)
        {
            DawnItemInfo? itemInfo = item.itemProperties.GetDawnInfo();
            if (itemInfo == null)
            {
                if (item.NetworkObject.IsSceneObject == null || !item.NetworkObject.IsSceneObject.Value)
                {
                    DawnPlugin.Logger.LogError($"Item: {item.name} doesn't have a DawnItemInfo; cannot save. Contact the developer of this item.");
                }
                continue;
            }

            string keyString = itemInfo.Key.ToString();

            if (!keyIndexLookup.TryGetValue(keyString, out int keyIndex))
            {
                keyIndex = keysList.Count;
                keysList.Add(keyString);
                keyIndexLookup[keyString] = keyIndex;
            }

            Debuggers.Items?.Log($"Saving item: {keyString} into save data.");

            JToken itemSave = 0;
            if (item.itemProperties.saveItemVariable)
            {
                itemSave = ((IDawnSaveData)item).GetDawnDataToSave() ?? 0;
            }

            JObject extraSaveData = DawnItemSaveEvents.Collect(item);
            JToken extraSaveToken = extraSaveData.HasValues ? extraSaveData : 0;

            Vector3 worldPos = item.transform.position;
            Vector3 savePos = new(worldPos.x, worldPos.y - item.itemProperties.verticalOffset + 0.02f, worldPos.z);
            Vector3 rotation = item.transform.rotation.eulerAngles;

            // Row format:
            // [ keyIndex, px, py, pz, rx, ry, rz, scrap, itemSavedData, extraData ]
            JArray row = new()
            {
                keyIndex,          // 0
                savePos.x,         // 1
                savePos.y,         // 2
                savePos.z,         // 3
                rotation.x,        // 4
                rotation.y,        // 5
                rotation.z,        // 6
                item.scrapValue,   // 7
                itemSave,          // 8
                extraSaveToken     // 9
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
            dataContainer.Set(DawnKeys.ShipItemsSaveData, root);
        }
    }

    private static void LoadFromRoot(JObject root)
    {
        JArray keysArray = root["keys"] as JArray ?? [];
        JArray itemsArray = root["items"] as JArray ?? [];

        foreach (JToken token in itemsArray)
        {
            if (token is not JArray row || row.Count < 8)
            {
                DawnPlugin.Logger.LogWarning("Malformed ship item row in save data; skipping.");
                continue;
            }

            int keyIndex = row[0].ToObject<int>();
            if (keyIndex < 0 || keyIndex >= keysArray.Count)
            {
                DawnPlugin.Logger.LogWarning($"Invalid key index {keyIndex} in save; skipping.");
                continue;
            }

            string? keyString = keysArray[keyIndex].ToObject<string>();
            if (string.IsNullOrWhiteSpace(keyString) || !NamespacedKey.TryParse(keyString, out NamespacedKey? itemKey))
            {
                DawnPlugin.Logger.LogWarning($"Invalid item key '{keyString}' in save; skipping.");
                continue;
            }

            if (!LethalContent.Items.TryGetValue(itemKey, out DawnItemInfo itemInfo))
            {
                DawnPlugin.Logger.LogWarning($"Item: {itemKey} no longer exists (mod removed?); skipping.");
                continue;
            }

            float px = row[1].ToObject<float>();
            float py = row[2].ToObject<float>();
            float pz = row[3].ToObject<float>();
            Vector3 spawnPosition = new(px, py, pz);

            float rx = row[4].ToObject<float>();
            float ry = row[5].ToObject<float>();
            float rz = row[6].ToObject<float>();
            Vector3 rotation = new(rx, ry, rz);

            int scrap = row[7].ToObject<int>();

            JToken itemSavedData = row.Count > 8 ? row[8] : 0;
            JToken extraSaveData = row.Count > 9 ? row[9] : 0;

            AddColliderForItemSaving(StartOfRoundRefs.Instance.shipInnerRoomBounds);
            ValidateColliderEntries();
            bool validSpawn = false;
            foreach (Collider collider in ValidItemSpawningColliders)
            {
                Bounds bounds = collider.bounds;
                if (bounds.Contains(spawnPosition))
                {
                    validSpawn = true;
                    break;
                }
            }

            if (!validSpawn)
            {
                spawnPosition = StartOfRoundRefs.Instance.playerSpawnPositions[1].position;
            }

            GrabbableObject grabbable = GameObject.Instantiate(
                itemInfo.Item.spawnPrefab,
                spawnPosition,
                Quaternion.Euler(rotation),
                StartOfRoundRefs.Instance.elevatorTransform).GetComponent<GrabbableObject>();

            grabbable.fallTime = 0f;
            grabbable.scrapPersistedThroughRounds = true;
            grabbable.isInElevator = true;
            grabbable.isInShipRoom = true;
            grabbable.SetScrapValue(scrap);

            if (grabbable.itemProperties.saveItemVariable && itemSavedData is not null && itemSavedData.Type != JTokenType.Null && !(itemSavedData.Type == JTokenType.Integer && itemSavedData.Value<int>() == 0))
            {
                ((IDawnSaveData)grabbable).LoadDawnSaveData(itemSavedData);
            }

            grabbable.NetworkObject.Spawn(false);
            grabbable.NetworkObject.OnSpawn(() =>
            {
                if (extraSaveData is not null && extraSaveData.Type != JTokenType.Null && !(extraSaveData.Type == JTokenType.Integer && extraSaveData.Value<int>() == 0))
                {
                    DawnItemSaveEvents.Restore(grabbable, extraSaveData);
                }
            });
            StartOfRound.Instance.StartCoroutine(EnsureItemRotatedCorrectly(grabbable.transform, rotation));
        }
    }

    private static JObject? TryGetCurrentFormat(PersistentDataContainer dataContainer)
    {
        JObject root;

        try
        {
            root = dataContainer.GetOrCreateDefault<JObject>(DawnKeys.ShipItemsSaveData);
        }
        catch (Exception e)
        {
            DawnPlugin.Logger.LogWarning($"Failed to read ship items save data. ({e.GetType().Name}: {e.Message})");
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

    private static IEnumerator EnsureItemRotatedCorrectly(Transform transform, Vector3 rotation)
    {
        yield return null;
        yield return null;
        yield return null;
        transform.rotation = Quaternion.Euler(rotation);
    }
}

public static class DawnItemSaveEvents
{
    public static event Action<GrabbableObject, JObject>? OnCollectExtraSaveData;
    public static event Action<GrabbableObject, JObject>? OnLoadExtraSaveData;

    internal static JObject Collect(GrabbableObject item)
    {
        JObject obj = new();
        OnCollectExtraSaveData?.Invoke(item, obj);
        return obj;
    }

    internal static void Restore(GrabbableObject item, JToken? token)
    {
        if (token is not JObject obj)
            return;

        OnLoadExtraSaveData?.Invoke(item, obj);
    }
}