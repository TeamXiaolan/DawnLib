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

namespace CodeRebirthLib;
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(LethalLib.Plugin.ModGUID, BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(WeatherRegistry.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(LethalConfig.PluginInfo.Guid, BepInDependency.DependencyFlags.SoftDependency)]
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
        
        if (Chainloader.PluginInfos.ContainsKey(LethalConfig.PluginInfo.Guid))
        {
            // try patches.
            LethalConfigPatch.Patch();
        }

        if (WeatherRegistryCompatibility.Enabled)
        {
            WeatherRegistryCompatibility.Init();
        }
        
        ExtendedTOML.Init();
        MoreLayerMasks.Init();
        
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