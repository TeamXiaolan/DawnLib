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

    internal static void LoadSavedItems(PersistentDataContainer dataContainer)
    {
        List<ItemSaveData> itemSaveDataList = dataContainer.GetOrCreateDefault<List<ItemSaveData>>(_namespacedKey);
        foreach (ItemSaveData itemData in itemSaveDataList)
        {
            Debuggers.SaveManager?.Log($"Loading item: {itemData.ItemNamespacedKey} from save data with information: {itemData.SavedSpawnPosition}, {itemData.SavedSpawnRotation}, {itemData.ScrapValue}, {itemData.ItemSavedData}.");
            if (!LethalContent.Items.TryGetValue(itemData.ItemNamespacedKey, out DawnItemInfo itemInfo))
            {
                DawnPlugin.Logger.LogWarning($"Item: {itemData.ItemNamespacedKey} doesn't exist in the game, this means this item cannot be loaded from the savefile, presumably you removed a mod that added this time previously.");
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
            allShipItemDatas.Add(new ItemSaveData(itemInfo.Key, new Vector3(itemData.transform.position.x, itemData.transform.position.y - itemData.itemProperties.verticalOffset, itemData.transform.position.z), itemData.transform.rotation.eulerAngles, itemData.scrapValue, itemSave));
        }

        using (dataContainer.CreateEditContext())
        {
            dataContainer.Set(_namespacedKey, allShipItemDatas);
        }
    }

    public struct ItemSaveData(NamespacedKey itemNamespacedKey, Vector3 savePosition, Vector3 saveRotation, int scrapValue, JToken itemSavedData)
    {
        public NamespacedKey ItemNamespacedKey = itemNamespacedKey;
        [JsonConverter(typeof(Vector3Converter))] public Vector3 SavedSpawnPosition = savePosition;
        [JsonConverter(typeof(Vector3Converter))] public Vector3 SavedSpawnRotation = saveRotation;
        public int ScrapValue = scrapValue;
        public JToken ItemSavedData = itemSavedData;
    }
}