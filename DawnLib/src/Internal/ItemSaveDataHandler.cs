using System.Collections;
using System.Collections.Generic;
using Dawn.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Dawn.Internal;
public static class ItemSaveDataHandler
{
    internal static GrabbableObject[] AllShipItems = [];
    private static NamespacedKey _namespacedKey = NamespacedKey.From("dawn_lib", "ship_items_save_data");
    private static NamespacedKey _itemKeyMapNamespacedKey = NamespacedKey.From("dawn_lib", "ship_items_key_map");

    internal static void LoadSavedItems(PersistentDataContainer dataContainer)
    {
        List<ItemSaveData> itemSaveDataList = dataContainer.GetOrCreateDefault<List<ItemSaveData>>(_namespacedKey);
        Dictionary<ushort, NamespacedKey> itemKeyMap = dataContainer.GetOrCreateDefault<Dictionary<ushort, NamespacedKey>>(_itemKeyMapNamespacedKey);
        foreach (ItemSaveData itemData in itemSaveDataList)
        {
            if (!itemKeyMap.TryGetValue(itemData.ItemKeyId, out NamespacedKey itemNamespacedKey))
            {
                DawnPlugin.Logger.LogWarning($"Item key ID {itemData.ItemKeyId} not found in key map, skipping item load.");
                continue;
            }
            Debuggers.SaveManager?.Log($"Loading item: {itemNamespacedKey} from save data with information: {itemData.SavedSpawnPosition}, {itemData.SavedSpawnRotation}, {itemData.ScrapValue}, {itemData.ItemSavedData}.");
            if (!LethalContent.Items.TryGetValue(itemNamespacedKey, out DawnItemInfo itemInfo))
            {
                DawnPlugin.Logger.LogWarning($"Item: {itemNamespacedKey} doesn't exist in the game, this means this item cannot be loaded from the savefile, presumably you removed a mod that added this time previously.");
                continue;
            }
            Vector3 spawnPosition = itemData.SavedSpawnPosition;
            if (!StartOfRoundRefs.Instance.shipBounds.bounds.Contains(spawnPosition))
            {
                spawnPosition = StartOfRoundRefs.Instance.playerSpawnPositions[1].position;
            }
            GrabbableObject grabbableObject = Object.Instantiate(itemInfo.Item.spawnPrefab, spawnPosition, Quaternion.Euler(itemData.SavedSpawnRotation), StartOfRoundRefs.Instance.elevatorTransform).GetComponent<GrabbableObject>();
            grabbableObject.fallTime = 0f;
            grabbableObject.scrapPersistedThroughRounds = true;
            grabbableObject.isInElevator = true;
            grabbableObject.isInShipRoom = true;
            grabbableObject.SetScrapValue(itemData.ScrapValue);
            if (grabbableObject.itemProperties.saveItemVariable)
            {
                ((IDawnSaveData)grabbableObject).LoadDawnSaveData(itemData.ItemSavedData);
            }
            grabbableObject.NetworkObject.Spawn(false);
            StartOfRound.Instance.StartCoroutine(EnsureItemRotatedCorrectly(grabbableObject.transform, itemData.SavedSpawnRotation));
        }
    }

    private static IEnumerator EnsureItemRotatedCorrectly(Transform transform, Vector3 rotation)
    {
        yield return null;
        yield return null;
        yield return null;
        transform.rotation = Quaternion.Euler(rotation);
    }

    internal static void SaveAllItems(PersistentDataContainer dataContainer)
    {
        AllShipItems = GameObject.FindObjectsOfType<GrabbableObject>();
        List<ItemSaveData> allShipItemDatas = new();
        Dictionary<NamespacedKey, ushort> itemKeyToIdMap = new();
        ushort nextId = 0;
        
        foreach (GrabbableObject itemData in AllShipItems)
        {
            DawnItemInfo? itemInfo = itemData.itemProperties.GetDawnInfo();
            if (itemInfo == null)
            {
                DawnPlugin.Logger.LogError($"Item: {itemData.name} doesn't have a DawnItemInfo, this means this item cannot be committed to the savefile, contact the developer of this item to fix this ASAP!");
                continue;
            }
            Debuggers.Items?.Log($"Saving item: {itemInfo.Key} into save data.");
            JToken itemSave = 0;
            if (itemData.itemProperties.saveItemVariable)
            {
                itemSave = ((IDawnSaveData)itemData).GetDawnDataToSave();
            }
            
            if (!itemKeyToIdMap.TryGetValue(itemInfo.Key, out ushort itemId))
            {
                itemId = nextId++;
                itemKeyToIdMap[itemInfo.Key] = itemId;
            }
            
            allShipItemDatas.Add(new ItemSaveData(itemId, new Vector3(itemData.transform.position.x, itemData.transform.position.y - itemData.itemProperties.verticalOffset + 0.02f, itemData.transform.position.z), itemData.transform.rotation.eulerAngles, itemData.scrapValue, itemSave));
        }

        using (dataContainer.CreateEditContext())
        {
            Dictionary<ushort, NamespacedKey> idToKeyMap = new Dictionary<ushort, NamespacedKey>();
            foreach (var kvp in itemKeyToIdMap)
            {
                idToKeyMap[kvp.Value] = kvp.Key;
            }
            
            dataContainer.Set(_namespacedKey, allShipItemDatas);
            dataContainer.Set(_itemKeyMapNamespacedKey, idToKeyMap);
        }
    }

    public struct ItemSaveData(ushort itemKeyId, Vector3 savePosition, Vector3 saveRotation, int scrapValue, JToken itemSavedData)
    {
        public ushort ItemKeyId = itemKeyId;
        [JsonConverter(typeof(Vector3Converter))] public Vector3 SavedSpawnPosition = savePosition;
        [JsonConverter(typeof(Vector3Converter))] public Vector3 SavedSpawnRotation = saveRotation;
        public int ScrapValue = scrapValue;
        public JToken ItemSavedData = itemSavedData;
    }
}