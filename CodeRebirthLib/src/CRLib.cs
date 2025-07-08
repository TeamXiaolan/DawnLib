using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using CodeRebirthLib.AssetManagement;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.ContentManagement;
using CodeRebirthLib.Util;
using CodeRebirthLib.Util.INetworkSerializables;
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
        CodeRebirthLibPlugin.ExtendedLogging("Registering no-code mod!");
        ConfigManager configManager = new(GenerateConfigFile(plugin));
        CRMod noCodeMod = new(plugin, mainBundle, basePath, configManager);
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
}