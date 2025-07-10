using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using CodeRebirthLib.AssetManagement;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.ContentManagement.Unlockables.Progressive;
using CodeRebirthLib.MiscScriptManagement;
using CodeRebirthLib.ModCompats;
using CodeRebirthLib.Patches;
using CodeRebirthLib.Util;
using CodeRebirthLib.Util.Pathfinding;
using LethalLib;
using PathfindingLib;
using UnityEngine;
using PluginInfo = WeatherRegistry.PluginInfo;

namespace CodeRebirthLib;
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(Plugin.ModGUID)]
[BepInDependency(PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(LethalConfig.PluginInfo.Guid, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(PathfindingLibPlugin.PluginGUID)]
class CodeRebirthLibPlugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static ConfigManager ConfigManager { get; private set; } = null!;
    internal static MainAssets Main { get; private set; } = null!;

    private void Awake()
    {
        Logger = base.Logger;
        ConfigManager = new ConfigManager(Config);

        CodeRebirthLibConfig.Bind(ConfigManager);

        NetcodePatcher();
        RoundManagerPatch.Patch();
        GameNetworkManagerPatch.Init();
        EnemyAIPatch.Init();
        StartOfRoundPatch.Init();
        TerminalPatch.Init();
        DeleteFileButtonPatch.Init();

        if (LethalConfigCompatibility.Enabled)
        {
            LethalConfigCompatibility.Init();
        }

        if (WeatherRegistryCompatibility.Enabled)
        {
            WeatherRegistryCompatibility.Init();
        }

        ExtendedTOML.Init();

        Main = new MainAssets(CRLib.LoadBundle(Assembly.GetExecutingAssembly(), "coderebirthlibmain"));

        foreach (string path in Directory.GetFiles(Paths.PluginPath, "*.crmod", SearchOption.AllDirectories))
        {
            Logger.LogFatal("Loading mod: " + Path.GetFileName(path));
            AssetBundle mainBundle = AssetBundle.LoadFromFile(path);
            ExtendedLogging($"[AssetBundle Loading] {mainBundle.name} contains these objects: {string.Join(",", mainBundle.GetAllAssetNames())}");
            
            CRModInformation[] modInformation = mainBundle.LoadAllAssets<CRModInformation>();
            if (modInformation.Length == 0)
            {
                Logger.LogError($".crmod bundle: '{Path.GetFileName(path)}' does not have a 'Mod Information' file!");
                continue;
            }

            if (modInformation.Length >= 1)
            {
                Logger.LogError($".crmod bundle: '{Path.GetFileName(path)}' has multiple 'Mod Information' files! Only the first one will be used.");
            }

            Logger.LogInfo($"AuthorName: {modInformation[0].AuthorName}, ModName: {modInformation[0].ModName}, Version: {modInformation[0].Version}");
            CRLib.RegisterNoCodeMod(modInformation[0].CreatePluginMetadata(), mainBundle, Path.GetDirectoryName(path)!);
        }

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    private void NetcodePatcher()
    {
        var types = new Type[] { typeof(UnlockShipUnlockable), typeof(SmartAgentNavigator), typeof(CodeRebirthLibNetworker), typeof(ClientNetworkTransform), typeof(OwnerNetworkAnimator) };
        foreach (var type in types)
        {
            try
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogWarning($"supressed an error from netcode patcher, probably fine but should still log that something happened: {e}.");
            }
        }
    }

    internal static void ExtendedLogging(object text)
    {
        if (CodeRebirthLibConfig.ExtendedLogging)
        {
            Logger.LogInfo(text);
        }
    }

    internal class MainAssets(AssetBundle bundle) : AssetBundleLoader<MainAssets>(bundle)
    {
        [LoadFromBundle("CodeRebirthLibNetworker.prefab")]
        public GameObject NetworkerPrefab { get; private set; } = null!;
    }
}