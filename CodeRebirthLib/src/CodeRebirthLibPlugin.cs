using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace CodeRebirthLib;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
class CodeRebirthLibPlugin : BaseUnityPlugin {
	internal new static ManualLogSource Logger { get; private set; } = null!;

	private void Awake() {
		Logger = base.Logger;

		NetcodePatcher();
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), MyPluginInfo.PLUGIN_GUID);

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
        if (true) // todo
        {
            Logger.LogInfo(text);
        }
    }
}