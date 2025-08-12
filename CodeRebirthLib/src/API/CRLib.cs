using System;
using CodeRebirthLib.CRMod;
using CodeRebirthLib.Dungeons;
using DunGen;
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
        TilesetInfoBuilder builder = new TilesetInfoBuilder(key, tileSet);
        callback(builder);
        LethalContent.TileSets.Register(builder.Build());
    }

    public static void DefineMapObject(NamespacedKey<CRMapObjectInfo> key, GameObject mapObject, Action<MapObjectInfoBuilder> callback)
    {
        MapObjectInfoBuilder builder = new MapObjectInfoBuilder(key, mapObject);
        callback(builder);
        LethalContent.MapObjects.Register(builder.Build());
    }

    public static void DefineUnlockable(NamespacedKey<CRUnlockableItemInfo> key, UnlockableItem unlockableItem, Action<UnlockableInfoBuilder> callback)
    {
        UnlockableInfoBuilder builder = new(key, unlockableItem);
        callback(builder);
        LethalContent.Unlockables.Register(builder.Build());
    }

    public static void DefineItem(NamespacedKey<CRItemInfo> key, Item item, Action<ItemInfoBuilder> callback)
    {
        ItemInfoBuilder builder = new ItemInfoBuilder(key, item);
        callback(builder);
        LethalContent.Items.Register(builder.Build());
    }

    public static void DefineEnemy(NamespacedKey<CREnemyInfo> key, EnemyType enemy, Action<EnemyInfoBuilder> callback)
    {
        EnemyInfoBuilder builder = new(key, enemy);
        callback(builder);
        LethalContent.Enemies.Register(builder.Build());
    }

    public static TerminalNodeBuilder DefineTerminalNode(string name)
    {
        return new TerminalNodeBuilder(name);
    }
}