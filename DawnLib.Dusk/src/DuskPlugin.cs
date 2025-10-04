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
    internal static PersistentDataContainer PersistentData { get; private set; } = null!;

    internal static MainAssets Main { get; private set; } = null!;

    private void Awake()
    {
        Logger = base.Logger;
        PersistentData = this.GetPersistentDataContainer();
        Logger.LogInfo("Doing patches");

        AchievementRegistrationPatch.Init();
        NetworkerPatch.Init();
        VehicleRegistrationPatch.Init();
        EntityReplacementRegistrationPatch.Init();

        Logger.LogInfo("Loading assets");
        Main = new MainAssets(AssetBundleUtils.LoadBundle(Assembly.GetExecutingAssembly(), "dawnlibmain"));
        
        Logger.LogInfo("Registering auto DuskMods!");
        AutoDuskModHandler.AutoRegisterMods();
        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
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