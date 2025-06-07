using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.Data;
using CodeRebirthLib.Patches;
using UnityEngine;

namespace CodeRebirthLib;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("ainavt.lc.lethalconfig", BepInDependency.DependencyFlags.SoftDependency)]
class CodeRebirthLibPlugin : BaseUnityPlugin {
	internal new static ManualLogSource Logger { get; private set; } = null!;

    internal static ConfigManager ConfigManager { get; private set; } = null!;
    
	private void Awake() {
		Logger = base.Logger;
        ConfigManager = new ConfigManager(Config);

        CodeRebirthLibConfig.Bind(ConfigManager);
        
		NetcodePatcher();
        RoundManagerPatch.Patch();
        
        if (Chainloader.PluginInfos.ContainsKey("ainavt.lc.lethalconfig"))
        {
            // try patches.
            LethalConfigPatch.Patch();
        }
        
        ExtendedTOML.Init();
        
		Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
	}
    
	private void NetcodePatcher() {
		var types = Assembly.GetExecutingAssembly().GetTypes();
		foreach(var type in types) {
			var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
			foreach(var method in methods) {
				var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
				if(attributes.Length > 0) {
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