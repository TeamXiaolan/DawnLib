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

        AdditionalTilesRegistrationHandler.tilesToFixSockets.Add(prefab);
    }

    public static void DefineTileSet(NamespacedKey<CRTileSetInfo> key, TileSet tileSet, Action<TilesetInfoBuilder> callback)
    {
        TilesetInfoBuilder builder = new(key, tileSet);
        callback(builder);
        CRTileSetInfo tileSetInfo = builder.Build();
        tileSet.SetCRInfo(tileSetInfo);
        LethalContent.TileSets.Register(tileSetInfo);
    }

    public static void DefineMapObject(NamespacedKey<CRMapObjectInfo> key, GameObject mapObject, Action<MapObjectInfoBuilder> callback)
    {
        MapObjectInfoBuilder builder = new(key, mapObject);
        callback(builder);
        // TODO what to do here?
        LethalContent.MapObjects.Register(builder.Build());
    }

    public static void DefineUnlockable(NamespacedKey<CRUnlockableItemInfo> key, UnlockableItem unlockableItem, Action<UnlockableInfoBuilder> callback)
    {
        UnlockableInfoBuilder builder = new(key, unlockableItem);
        callback(builder);
        CRUnlockableItemInfo unlockableItemInfo = builder.Build();
        unlockableItem.SetCRInfo(unlockableItemInfo);
        LethalContent.Unlockables.Register(unlockableItemInfo);
    }

    public static void DefineItem(NamespacedKey<CRItemInfo> key, Item item, Action<ItemInfoBuilder> callback)
    {
        ItemInfoBuilder builder = new(key, item);
        callback(builder);
        CRItemInfo itemInfo = builder.Build();
        item.SetCRInfo(itemInfo);
        LethalContent.Items.Register(itemInfo);
    }

    public static void DefineEnemy(NamespacedKey<CREnemyInfo> key, EnemyType enemy, Action<EnemyInfoBuilder> callback)
    {
        EnemyInfoBuilder builder = new(key, enemy);
        callback(builder);
        CREnemyInfo enemyInfo = builder.Build();
        enemy.SetCRInfo(enemyInfo);
        LethalContent.Enemies.Register(enemyInfo);
    }

    public static void ApplyTag(JSONTagDefinition definition)
    {
        NamespacedKey tag = NamespacedKey.Parse(definition.Tag);

        TagRegistrationHandler.OnApplyTags += () =>
        {
            Debuggers.Tags?.Log($"Applying tag: {tag}");

            foreach (string value in definition.Values)
            {
                NamespacedKey key = NamespacedKey.Parse(value);
                // this isn't great lmao
                if (LethalContent.Moons.TryGetValue(key, out CRMoonInfo moonInfo)) moonInfo.Internal_AddTag(tag);
                if (LethalContent.Weathers.TryGetValue(key, out CRWeatherEffectInfo weatherInfo)) weatherInfo.Internal_AddTag(tag);
                if (LethalContent.Enemies.TryGetValue(key, out CREnemyInfo enemyInfo)) enemyInfo.Internal_AddTag(tag);
                if (LethalContent.MapObjects.TryGetValue(key, out CRMapObjectInfo mapObjectInfo)) mapObjectInfo.Internal_AddTag(tag);
                if (LethalContent.Items.TryGetValue(key, out CRItemInfo itemInfo)) itemInfo.Internal_AddTag(tag);
                if (LethalContent.Dungeons.TryGetValue(key, out CRDungeonInfo dungeonInfo)) dungeonInfo.Internal_AddTag(tag);
                if (LethalContent.TileSets.TryGetValue(key, out CRTileSetInfo tileSetInfo)) tileSetInfo.Internal_AddTag(tag);
                if (LethalContent.Unlockables.TryGetValue(key, out CRUnlockableItemInfo unlockableItemInfo)) unlockableItemInfo.Internal_AddTag(tag);
            }
        };
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