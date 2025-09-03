using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using LethalLib;
using UnityEngine;
using static LethalLib.Modules.Enemies;
using static LethalLib.Modules.Items;
using static LethalLib.Modules.MapObjects;
using static LethalLib.Modules.Unlockables;
using static LethalLib.Modules.Weathers;

namespace Dawn.Internal;

static class LethalLibCompat
{
    public static bool Enabled => Chainloader.PluginInfos.ContainsKey(Plugin.ModGUID);

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool TryGetWeatherEffectFromLethalLib(string weatherName, out string modName)
    {
        modName = "lethal_lib";
        foreach (CustomWeather customWeather in customWeathers.Values)
        {
            if (customWeather.name == weatherName)
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool TryGetUnlockableItemFromLethalLib(UnlockableItem unlockableItem, out string modName)
    {
        modName = "lethal_lib";
        foreach (RegisteredUnlockable registeredUnlockable in registeredUnlockables)
        {
            if (registeredUnlockable.unlockable == unlockableItem)
            {
                modName = registeredUnlockable.modName;
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool TryGetItemFromLethalLib(Item item, out string modName)
    {
        modName = "lethal_lib";
        foreach (ScrapItem scrapItem in scrapItems)
        {
            if (scrapItem.item == item)
            {
                modName = scrapItem.modName;
                return true;
            }
        }

        foreach (ShopItem shopItem in shopItems)
        {
            if (shopItem.item == item)
            {
                modName = shopItem.modName;
                return true;
            }
        }

        foreach (PlainItem plainItem in plainItems)
        {
            if (plainItem.item == item)
            {
                modName = plainItem.modName;
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool TryGetEnemyTypeFromLethalLib(EnemyType enemyType, out string modName)
    {
        modName = "lethal_lib";
        foreach (SpawnableEnemy spawnableEnemy in spawnableEnemies)
        {
            if (spawnableEnemy.enemy == enemyType)
            {
                modName = spawnableEnemy.modName;
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool TryGetMapObjectFromLethalLib(GameObject mapObject, out string modName)
    {
        modName = "lethal_lib";
        foreach (RegisteredMapObject registeredMapObject in mapObjects)
        {
            if (registeredMapObject.mapObject != null && registeredMapObject.mapObject.prefabToSpawn == mapObject)
            {
                return true;
            }
            else if (registeredMapObject.outsideObject != null && registeredMapObject.outsideObject.spawnableObject.prefabToSpawn == mapObject)
            {
                return true;
            }
        }
        return false;
    }
}