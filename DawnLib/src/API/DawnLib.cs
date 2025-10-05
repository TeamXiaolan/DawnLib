using System;
using System.IO;
using Dawn.Internal;
using DunGen;
using DunGen.Graph;
using Newtonsoft.Json;
using UnityEngine;

namespace Dawn;
public static class DawnLib
{
    public const string PLUGIN_GUID = MyPluginInfo.PLUGIN_GUID;

    internal static readonly JsonSerializerSettings JSONSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All,
        Formatting = Formatting.Indented,
        Converters =
        [
            new NamespacedKeyConverter(),
            new NamespacedKeyDictionaryConverter()
        ]
    };

    /// <summary>
    /// This save is reset ONLY when deleting the file and remains between getting fired.
    /// </summary>
    /// <remarks>Note that this is not synced between players.</remarks>
    /// <returns>The data container or null if not in-game</returns>
    public static PersistentDataContainer? GetCurrentSave()
    {
        return DawnNetworker.Instance?.SaveContainer;
    }

    /// <summary>
    /// This save is reset on deleting the file AND getting fired.
    /// </summary>
    /// <remarks>Note that this is not synced between players.</remarks>
    /// <returns>The data container or null if not in-game</returns>
    public static PersistentDataContainer? GetCurrentContract()
    {
        return DawnNetworker.Instance?.ContractContainer;
    }

    public static void RegisterNetworkPrefab(GameObject prefab)
    {
        if (!prefab)
            throw new ArgumentNullException(nameof(prefab));

        MiscFixesPatch.networkPrefabsToAdd.Add(prefab);
    }

    public static void RegisterNetworkScene(string scenePath)
    {
        DawnNetworkSceneManager.AddScenePath(scenePath);
    }
    
    public static void FixMixerGroups(GameObject prefab)
    {
        if (!prefab)
            throw new ArgumentNullException(nameof(prefab));

        MiscFixesPatch.soundPrefabsToFix.Add(prefab);
    }

    public static void FixDoorwaySockets(GameObject prefab)
    {
        if (!prefab)
            throw new ArgumentNullException(nameof(prefab));

        MiscFixesPatch.tilesToFixSockets.Add(prefab);
    }

    public static DawnDungeonInfo DefineDungeon(NamespacedKey<DawnDungeonInfo> key, DungeonFlow dungeonFlow, Action<DungeonFlowInfoBuilder> callback)
    {
        DungeonFlowInfoBuilder builder = new(key, dungeonFlow);
        callback(builder);
        DawnDungeonInfo dungeonFlowInfo = builder.Build();
        dungeonFlow.SetDawnInfo(dungeonFlowInfo);
        LethalContent.Dungeons.Register(dungeonFlowInfo);
        return dungeonFlowInfo;
    }

    public static DawnTileSetInfo DefineTileSet(NamespacedKey<DawnTileSetInfo> key, TileSet tileSet, Action<TilesetInfoBuilder> callback)
    {
        TilesetInfoBuilder builder = new(key, tileSet);
        callback(builder);
        DawnTileSetInfo tileSetInfo = builder.Build();
        tileSet.SetDawnInfo(tileSetInfo);
        LethalContent.TileSets.Register(tileSetInfo);
        return tileSetInfo;
    }

    public static DawnMapObjectInfo DefineMapObject(NamespacedKey<DawnMapObjectInfo> key, GameObject mapObject, Action<MapObjectInfoBuilder> callback)
    {
        MapObjectInfoBuilder builder = new(key, mapObject);
        callback(builder);
        DawnMapObjectInfo info = builder.Build();
        DawnMapObjectInfoContainer container = mapObject.AddComponent<DawnMapObjectInfoContainer>();
        container.Value = info;

        LethalContent.MapObjects.Register(info);
        return info;
    }

    public static DawnUnlockableItemInfo DefineUnlockable(NamespacedKey<DawnUnlockableItemInfo> key, UnlockableItem unlockableItem, Action<UnlockableInfoBuilder> callback)
    {
        UnlockableInfoBuilder builder = new(key, unlockableItem);
        callback(builder);
        DawnUnlockableItemInfo unlockableItemInfo = builder.Build();
        unlockableItem.SetDawnInfo(unlockableItemInfo);
        LethalContent.Unlockables.Register(unlockableItemInfo);
        return unlockableItemInfo;
    }

    public static DawnItemInfo DefineItem(NamespacedKey<DawnItemInfo> key, Item item, Action<ItemInfoBuilder> callback)
    {
        ItemInfoBuilder builder = new(key, item);
        callback(builder);
        DawnItemInfo itemInfo = builder.Build();
        item.SetDawnInfo(itemInfo);
        LethalContent.Items.Register(itemInfo);
        return itemInfo;
    }

    public static DawnEnemyInfo DefineEnemy(NamespacedKey<DawnEnemyInfo> key, EnemyType enemy, Action<EnemyInfoBuilder> callback)
    {
        EnemyInfoBuilder builder = new(key, enemy);
        callback(builder);
        DawnEnemyInfo enemyInfo = builder.Build();
        enemy.SetDawnInfo(enemyInfo);
        LethalContent.Enemies.Register(enemyInfo);
        return enemyInfo;
    }

    public static void ApplyTag(JSONTagDefinition definition)
    {
        void ListenToRegistry<T>(TaggedRegistry<T> registry, NamespacedKey namespacedKey) where T : DawnBaseInfo<T>
        {
            registry.OnFreeze += () =>
            {
                foreach (string value in definition.Values)
                {
                    if (registry.TryGetValue(NamespacedKey.Parse(value), out T info))
                    {
                        info.Internal_AddTag(namespacedKey);
                    }
                }
            };
        }

        NamespacedKey tag = NamespacedKey.Parse(definition.Tag);

        ListenToRegistry(LethalContent.Moons, tag);
        ListenToRegistry(LethalContent.MapObjects, tag);
        ListenToRegistry(LethalContent.Enemies, tag);
        ListenToRegistry(LethalContent.Items, tag);
        ListenToRegistry(LethalContent.Weathers, tag);
        ListenToRegistry(LethalContent.Dungeons, tag);
        ListenToRegistry(LethalContent.Unlockables, tag);

        Debuggers.Tags?.Log($"Scheduled applying tag: {tag}");
    }

    public static void ApplyAllTagsInFolder(string path)
    {
        foreach (string filePath in Directory.GetFiles(path, "*.tag.json", SearchOption.AllDirectories))
        {
            JSONTagDefinition definition = JsonConvert.DeserializeObject<JSONTagDefinition>(File.ReadAllText(filePath))!;
            ApplyTag(definition);
        }
    }
}