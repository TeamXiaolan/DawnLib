using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using Dawn;
using Dawn.Utils;
using Dusk.Internal;
using Dusk.Utils;
using UnityEngine;

namespace Dusk;
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(DawnLib.PLUGIN_GUID)]
public class DuskPlugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger { get; private set; } = null!;

    internal static MainAssets Main { get; private set; } = null!;
    
    private void Awake()
    {
        Logger = base.Logger;
        
        Logger.LogInfo("Doing patches");
        
        AchievementRegistrationPatch.Init();
        NetworkerPatch.Init();
        
        Logger.LogInfo("Loading assets");
        Main = new MainAssets(AssetBundleUtils.LoadBundle(Assembly.GetExecutingAssembly(), "dawnlibmain"));
        
        NetcodePatcher();
        
        Logger.LogInfo("Registering auto DuskMods!");
        AutoDuskModHandler.AutoRegisterMods();
        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }
    
    private void NetcodePatcher()
    {
        var types = new Type[] { typeof(UnlockProgressiveObject), typeof(ItemUpgradeScrap), typeof(UnlockableUpgradeScrap) };
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
    
    internal class MainAssets(AssetBundle bundle) : AssetBundleLoader<MainAssets>(bundle)
    {
        [LoadFromBundle("DawnLibNetworker.prefab")]
        public GameObject NetworkerPrefab { get; private set; } = null!;

        [LoadFromBundle("AchievementUICanvas.prefab")]
        public GameObject AchievementUICanvasPrefab { get; private set; } = null!;

        [LoadFromBundle("AchievementGetUICanvas.prefab")]
        public GameObject AchievementGetUICanvasPrefab { get; private set; } = null!;
    }
}