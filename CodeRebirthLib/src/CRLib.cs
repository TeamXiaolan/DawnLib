using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.ConfigManagement.Weights;
using CodeRebirthLib.ContentManagement;
using CodeRebirthLib.ContentManagement.Enemies;
using CodeRebirthLib.Data;
using CodeRebirthLib.Patches;
using CodeRebirthLib.Util;
using CodeRebirthLib.Util.INetworkSerializables;
using DunGen;
using Steamworks.Data;
using UnityEngine;

namespace CodeRebirthLib;

public static class CRLib
{
    public static AssetBundle LoadBundle(Assembly assembly, string filePath)
    {
        string correctPath = Path.Combine(Path.GetDirectoryName(assembly.Location), "Assets", filePath);

        if (!File.Exists(correctPath))
        {
            string incorrectPath = Path.Combine(Path.GetDirectoryName(assembly.Location), filePath);
            bool atIncorrectPath = File.Exists(incorrectPath);

            string message = $"The assetbundle at plugins/{Path.GetRelativePath(Paths.PluginPath, correctPath)} does not exist!";
            if (atIncorrectPath)
            {
                message += $" The bundle was found at the incorrect spot: plugins/{Path.GetRelativePath(Paths.PluginPath, incorrectPath)}. It should be within the Assets/ subfolder";
            }

            throw new FileNotFoundException(message);
        }

        return AssetBundle.LoadFromFile(correctPath);
    }

    public static CRMod RegisterMod(BaseUnityPlugin plugin, AssetBundle mainBundle)
    {
        ConfigManager configManager = new(plugin.Config);
        return new CRMod(plugin.GetType().Assembly, plugin, mainBundle, Path.GetDirectoryName(plugin.GetType().Assembly.Location)!, configManager);
    }

    public static void BroadcastTip(HUDDisplayTip displayTip)
    {
        CodeRebirthLibNetworker.Instance?.BroadcastDisplayTipServerRPC(displayTip);
    }

    public static void RegisterNetworkPrefab(GameObject prefab)
    {
        if (!prefab)
            throw new ArgumentNullException(nameof(prefab));

        GameNetworkManagerPatch.networkPrefabs.Add(prefab);
    }

    public static void FixMixerGroups(GameObject prefab)
    {
        if (!prefab)
            throw new ArgumentNullException(nameof(prefab));

        MenuManagerPatch.prefabsToFix.Add(prefab);
    }

    public static void FixDoorwaySockets(GameObject prefab)
    {
        if (!prefab)
            throw new ArgumentNullException(nameof(prefab));

        TileInjectionPatch.tilesToFixSockets.Add(prefab);
    }

    internal static CRMod RegisterNoCodeMod(CRModInformation modInfo, AssetBundle mainBundle, string basePath)
    {
        var plugin = modInfo.CreatePluginMetadata();
        CodeRebirthLibPlugin.ExtendedLogging("Registering no-code mod!");
        ConfigManager configManager = new(GenerateConfigFile(plugin));
        CRMod noCodeMod = new(plugin, mainBundle, basePath, configManager);
        noCodeMod.ModInformation = modInfo;
        noCodeMod.Logger = BepInEx.Logging.Logger.CreateLogSource(plugin.GUID);
        foreach (var assetBundleData in noCodeMod.Content.assetBundles)
        {
            _ = new DefaultContentHandler(noCodeMod);  // todo : i think i got pretty far in, there needs to be error handling for putting assetbundles next to eachother rather than mainbundle then Assets/subbundle
        }
        return noCodeMod;
    }

    internal static ConfigFile GenerateConfigFile(BepInPlugin plugin)
    {
        return new ConfigFile(Utility.CombinePaths(Paths.ConfigPath, plugin.GUID + ".cfg"), false, plugin);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="archetypeName">Name of the archetype SciptableObject, like Level1ArchetypeMaze</param>
    /// <param name="tileSet">TileSet to add</param>
    /// <param name="isBranchCap">Should this tileset be valid to spawn at the end of a corridor?</param>
    /// <remarks>You should also call <see cref="FixDoorwaySockets"/> on your tile prefabs</remarks>
    public static void RegisterTileSetForArchetype(string archetypeName, TileSet tileSet, bool isBranchCap = false)
    {
        TileInjectionPatch.AddTileSetForDungeon(archetypeName, new TileInjectionPatch.TileInjectionSettings(tileSet, isBranchCap)); // i want to keep a lot of the public facing methods in the CRLib class
    }
    
    public static void RegisterScrap(Item item, string levelName, int rarity)
    {
        RegisterScrap(item, levelName, new SimpleWeightProvider(rarity));
    }

    public static void RegisterScrap(Item item, string levelName, IWeighted provider)
    {
        CRItemsPatch.AddItemForLevel(levelName, new InjectionSettings<Item>(item, provider));
    }

    public static void RegisterScrap(Item item, Dictionary<string, int> levelRarities)
    {
        foreach ((string levelName, int rarity) in levelRarities)
        {
            RegisterScrap(item, levelName, rarity);
        }
    }
    
    // todo: register shop item?

    public static void InjectEnemyIntoLevel(SpawnTable spawnTable, SpawnWeightsPreset spawnWeights, EnemyType enemyType)
    {
        CREnemiesPatch.AddEnemyForLevel(spawnTable, spawnWeights, enemyType);
    }
}