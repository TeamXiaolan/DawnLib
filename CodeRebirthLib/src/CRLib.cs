using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.Util;
using UnityEngine;

namespace CodeRebirthLib;
public static class CRLib
{
    public static AssetBundle LoadBundle(Assembly assembly, string filePath)
    {
        return AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(assembly.Location), "Assets", filePath));
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

    internal static CRMod RegisterNoCodeMod(BepInPlugin plugin, AssetBundle mainBundle, string basePath)
    {
        ConfigManager configManager = new(GenerateConfigFile(plugin));
        return new CRMod(plugin, mainBundle, basePath, configManager);
    }

    internal static ConfigFile GenerateConfigFile(BepInPlugin plugin)
    {
        return new ConfigFile(Utility.CombinePaths(Paths.ConfigPath, plugin.GUID + ".cfg"), false, plugin);
    }
}