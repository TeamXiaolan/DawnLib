using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using Dawn;
using Dawn.Internal;
using Dawn.Utils;
using Dusk;
using Eventide.Internal;
using UnityEngine;

namespace Eventide;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(DawnLib.PLUGIN_GUID)]
[BepInDependency(DuskMod.PLUGIN_GUID)]
public class EventidePlugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static PersistentDataContainer PersistentData { get; private set; } = null!;

    internal static EventideMainAssets EventideMain { get; private set; } = null!;
    internal static EventideMainNetworkAssets EventideMainNetwork { get; private set; } = null!;

    internal static EventidePlugin Instance { get; private set; } = null!;

    private void Awake()
    {
        EventideConfig.Bind(Config);

        Instance = this;
        Logger = base.Logger;
        PersistentData = this.GetPersistentDataContainer();
        Logger.LogInfo("Doing patches");

        AchievementRegistrationPatch.Init();
        if (!DawnConfig.VanillaCompatibility.Value)
        {
            NetworkerPatch.Init();
        }

        Logger.LogInfo("Loading assets");

        EventideMain = new EventideMainAssets(AssetBundleUtils.LoadBundle(Assembly.GetExecutingAssembly(), "eventidemodmain"));
        if (!DawnConfig.VanillaCompatibility.Value)
        {
            EventideMainNetwork = new EventideMainNetworkAssets(AssetBundleUtils.LoadBundle(Assembly.GetExecutingAssembly(), "eventidemainnetwork"));
        }

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    internal class EventideMainAssets(AssetBundle bundle) : AssetBundleLoader<EventideMainAssets>(bundle)
    {
        [LoadFromBundle("AchievementUICanvas.prefab")]
        public GameObject AchievementUICanvasPrefab { get; private set; } = null!;

        [LoadFromBundle("AchievementGetUICanvas.prefab")]
        public GameObject AchievementGetUICanvasPrefab { get; private set; } = null!;
    }

    internal class EventideMainNetworkAssets(AssetBundle bundle) : AssetBundleLoader<EventideMainNetworkAssets>(bundle)
    {
        [LoadFromBundle("EventideNetworker.prefab")]
        public GameObject NetworkerPrefab { get; private set; } = null!;
    }
}