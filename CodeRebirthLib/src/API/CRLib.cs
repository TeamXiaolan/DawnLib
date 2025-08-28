using System;
using System.IO;
using CodeRebirthLib.CRMod;
using CodeRebirthLib.Internal;
using DunGen;
using Newtonsoft.Json;
using UnityEngine;

namespace CodeRebirthLib;
public static class CRLib
{
    public const string PLUGIN_GUID = MyPluginInfo.PLUGIN_GUID;

    public static void RegisterNetworkPrefab(GameObject prefab)
    {
        if (!prefab)
            throw new ArgumentNullException(nameof(prefab));

        MiscFixesPatch.networkPrefabsToAdd.Add(prefab);
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

    public static CRTileSetInfo DefineTileSet(NamespacedKey<CRTileSetInfo> key, TileSet tileSet, Action<TilesetInfoBuilder> callback)
    {
        TilesetInfoBuilder builder = new(key, tileSet);
        callback(builder);
        CRTileSetInfo tileSetInfo = builder.Build();
        tileSet.SetCRInfo(tileSetInfo);
        LethalContent.TileSets.Register(tileSetInfo);
        return tileSetInfo;
    }

    public static CRMapObjectInfo DefineMapObject(NamespacedKey<CRMapObjectInfo> key, GameObject mapObject, Action<MapObjectInfoBuilder> callback)
    {
        MapObjectInfoBuilder builder = new(key, mapObject);
        callback(builder);
        // TODO what to do here?
        CRMapObjectInfo info = builder.Build();
        LethalContent.MapObjects.Register(info);
        return info;
    }

    public static CRUnlockableItemInfo DefineUnlockable(NamespacedKey<CRUnlockableItemInfo> key, UnlockableItem unlockableItem, Action<UnlockableInfoBuilder> callback)
    {
        UnlockableInfoBuilder builder = new(key, unlockableItem);
        callback(builder);
        CRUnlockableItemInfo unlockableItemInfo = builder.Build();
        unlockableItem.SetCRInfo(unlockableItemInfo);
        LethalContent.Unlockables.Register(unlockableItemInfo);
        return unlockableItemInfo;
    }

    public static CRItemInfo DefineItem(NamespacedKey<CRItemInfo> key, Item item, Action<ItemInfoBuilder> callback)
    {
        ItemInfoBuilder builder = new(key, item);
        callback(builder);
        CRItemInfo itemInfo = builder.Build();
        item.SetCRInfo(itemInfo);
        LethalContent.Items.Register(itemInfo);
        return itemInfo;
    }

    public static CREnemyInfo DefineEnemy(NamespacedKey<CREnemyInfo> key, EnemyType enemy, Action<EnemyInfoBuilder> callback)
    {
        EnemyInfoBuilder builder = new(key, enemy);
        callback(builder);
        CREnemyInfo enemyInfo = builder.Build();
        enemy.SetCRInfo(enemyInfo);
        LethalContent.Enemies.Register(enemyInfo);
        return enemyInfo;
    }

    public static void ApplyTag(JSONTagDefinition definition)
    {
        void ListenToRegistry<T>(TaggedRegistry<T> registry, NamespacedKey namespacedKey) where T : CRBaseInfo<T> {
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
    
    public static TerminalNodeBuilder DefineTerminalNode(string name)
    {
        return new TerminalNodeBuilder(name);
    }
}