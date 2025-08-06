using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.ConfigManagement.Weights;
using CodeRebirthLib.ContentManagement;
using CodeRebirthLib.Patches;
using CodeRebirthLib.Util;
using CodeRebirthLib.Util.INetworkSerializables;
using DunGen;
using UnityEngine;
using UnityEngine.UI;

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

    public static void InjectTileSetForDungeon(string archetypeName, TileSet tileSet, bool isBranchCap = false)
    {
        TileInjectionPatch.AddTileSetForDungeon(archetypeName, new TileInjectionPatch.TileInjectionSettings(tileSet, isBranchCap)); // i want to keep a lot of the public facing methods in the CRLib class
    }

    public static void InjectItemIntoLevel(SpawnWeightsPreset spawnWeights, Item item)
    {
        CRItemsPatch.AddItemForLevel(spawnWeights, item);
    }
}