using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Dawn.Internal;
public static class ItemSaveDataHandler
{
    // TODO: patch all GrabbableObject LoadSaveData instances to run the vanilla LoadItemSaveData
    internal static List<GrabbableObject> AllShipItems = new();
    private static NamespacedKey _namespacedKey = NamespacedKey.From("dawn_lib", "ship_items_save_data");

    internal static void LoadSavedItems(IDataContainer dataContainer)
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
            /*GrabbableObject grabbableObject = Object.Instantiate(itemInfo.Item.spawnPrefab, spawnPosition, Quaternion.Euler(itemData.SavedSpawnRotation), StartOfRoundRefs.Instance.elevatorTransform).GetComponent<GrabbableObject>();
            grabbableObject.fallTime = 0f;
            grabbableObject.scrapPersistedThroughRounds = true;
            grabbableObject.isInElevator = true;
            grabbableObject.isInShipRoom = true;
            grabbableObject.SetScrapValue(itemData.ScrapValue);
            if (grabbableObject.itemProperties.saveItemVariable)
            {
                grabbableObject.LoadSaveData(itemData.ItemSavedData);
            }
            grabbableObject.NetworkObject.Spawn(false);*/
        }
    }

    internal static void SaveAllItems(PersistentDataContainer dataContainer)
    {
        AllShipItems = GameObject.FindObjectsOfType<GrabbableObject>().ToList();
        List<ItemSaveData> allShipItemDatas = new();
        foreach (GrabbableObject itemData in AllShipItems)
        {
            DawnItemInfo? itemInfo = itemData.itemProperties.GetDawnInfo();
            if (itemInfo == null)
            {
                DawnPlugin.Logger.LogError($"Item: {itemData.name} doesn't have a DawnItemInfo, this means this item cannot be committed to the savefile, contact the developer of this item to fix this ASAP!");
                continue;
            }
            Debuggers.Items?.Log($"Saving item: {itemData.itemProperties.GetDawnInfo().Key} into save data.");
            allShipItemDatas.Add(new ItemSaveData(itemInfo.Key, itemData.transform.position, itemData.transform.rotation.eulerAngles, itemData.scrapValue, itemData.GetSaveData()));
        }
        using (dataContainer.LargeEdit())
        {
            dataContainer.Set(_namespacedKey, allShipItemDatas);
        }
    }

    public struct ItemSaveData(NamespacedKey itemNamespacedKey, Vector3 savePosition, Vector3 saveRotation, int scrapValue, JToken? itemSavedData)
    {
        public NamespacedKey ItemNamespacedKey = itemNamespacedKey;
        public Vector3 SavedSpawnPosition = savePosition;
        public Vector3 SavedSpawnRotation = saveRotation;
        public int ScrapValue = scrapValue;
        public JToken ItemSavedData = itemSavedData ?? 0;
    }
}