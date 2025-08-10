using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeRebirthLib;
public static class CRLib
{
    public const string PLUGIN_GUID = MyPluginInfo.PLUGIN_GUID;

    public static void RegisterNetworkPrefab(GameObject prefab)
    {
        if (!prefab)
            throw new ArgumentNullException(nameof(prefab));

        GameNetworkManagerPatch.networkPrefabs.Add(prefab); // TODO maybe make a GameObjectFixesHandler or smthn
    }

    public static void FixMixerGroups(GameObject prefab)
    {
        if (!prefab)
            throw new ArgumentNullException(nameof(prefab));

        MenuManagerPatch.prefabsToFix.Add(prefab); // TODO maybe make a GameObjectFixesHandler or smthn
    }

    public static void FixDoorwaySockets(GameObject prefab)
    {
        if (!prefab)
            throw new ArgumentNullException(nameof(prefab));

        AdditionalTilesRegistrationHandler.tilesToFixSockets.Add(prefab); // TODO: make a method for this in the handler instead?
    }

    public static void DefineAchievement(NamespacedKey<CRAchievementInfo> key, Action<AchievementInfoBuilder> callback)
    {
        AchievementInfoBuilder builder = new AchievementInfoBuilder(key);
        callback(builder);
        LethalContent.Achievements.Register(builder.Build());
    }

    public static void DefineAdditionalTiles(NamespacedKey<CRAdditionalTileInfo> key, Action<AdditionalTileInfoBuilder> callback)
    {
        AdditionalTileInfoBuilder builder = new AdditionalTileInfoBuilder(key);
        callback(builder);
        LethalContent.AdditionalTiles.Register(builder.Build());
    }

    public static void DefineMapObject(NamespacedKey<CRMapObjectInfo> key, GameObject mapObject, Dictionary<NamespacedKey<CRMoonInfo>, AnimationCurve> animationCurveToLevelDict, Action<MapObjectInfoBuilder> callback)
    {
        MapObjectInfoBuilder builder = new MapObjectInfoBuilder(key, mapObject, animationCurveToLevelDict);
        callback(builder);
        LethalContent.MapObjects.Register(builder.Build());
    }

    public static void DefineUnlockable(NamespacedKey<CRUnlockableInfo> key, Action<UnlockableInfoBuilder> callback)
    {
        UnlockableInfoBuilder builder = new UnlockableInfoBuilder(key);
        callback(builder);
        LethalContent.Unlockables.Register(builder.Build());
    }

    public static void DefineWeather(NamespacedKey<CRWeatherInfo> key, Action<WeatherInfoBuilder> callback)
    {
        WeatherInfoBuilder builder = new WeatherInfoBuilder(key);
        callback(builder);
        LethalContent.Weather.Register(builder.Build());
    }

    public static void DefineItem(NamespacedKey<CRItemInfo> key, Item item, Action<ItemInfoBuilder> callback)
    {
        ItemInfoBuilder builder = new ItemInfoBuilder(key, item);
        callback(builder);
        LethalContent.Items.Register(builder.Build());
    }

    public static void DefineEnemy(NamespacedKey<CREnemyInfo> key, EnemyType enemy, Action<EnemyInfoBuilder> callback)
    {
        EnemyInfoBuilder builder = new EnemyInfoBuilder(key, enemy);
        callback(builder);
        LethalContent.Enemies.Register(builder.Build());
    }

    public static TerminalNodeBuilder DefineTerminalNode(string name)
    {
        return new TerminalNodeBuilder(name);
    }
}