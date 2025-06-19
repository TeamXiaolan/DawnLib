using System.IO;
using BepInEx;
using BepInEx.Logging;
using System.Reflection;
using BepInEx.Bootstrap;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.Patches;
using UnityEngine;
using CodeRebirthLib.Extensions;
using CodeRebirthLib.Util;
using CodeRebirthLib.ModCompats;
using Unity.Netcode;

namespace CodeRebirthLib;
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(LethalLib.Plugin.ModGUID)]
[BepInDependency(WeatherRegistry.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(LethalConfig.PluginInfo.Guid, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(PathfindingLib.PathfindingLibPlugin.PluginGUID)]
class CodeRebirthLibPlugin : BaseUnityPlugin
{
	internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static ConfigManager ConfigManager { get; private set; } = null!;
    
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
        
        foreach (string path in Directory.GetFiles(Paths.PluginPath, "*.crmod", SearchOption.AllDirectories))  
        {
            AssetBundle mainBundle = AssetBundle.LoadFromFile(path);
            CRModVersion modInformation = mainBundle.LoadAsset<CRModVersion>("Mod Information.asset");
            if (modInformation == null)
            {
                Logger.LogError($".crmod bundle: '{Path.GetFileName(path)}' does not have a 'Mod Information' file. Make sure you include one with that specific name!");
                continue;
            }

            CRLib.RegisterNoCodeMod(modInformation.CreatePluginMetadata(), mainBundle, Path.GetDirectoryName(path)!);
        }
      
		Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
	}
    
	private void NetcodePatcher()
	{
		var types = Assembly.GetExecutingAssembly().GetLoadableTypes();
		foreach (var type in types)
		{
            if (type.IsNested || !typeof(NetworkBehaviour).IsAssignableFrom(type))
            {
                continue; // we do not care about fixing it, if it is not a network behaviour
            }
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
	}
    
    internal static void ExtendedLogging(object text)
    {
        if (CodeRebirthLibConfig.ExtendedLogging)
        {
            Logger.LogInfo(text);
        }
    }
}