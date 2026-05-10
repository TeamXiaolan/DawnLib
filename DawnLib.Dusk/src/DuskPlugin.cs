using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using Dawn;
using Dawn.Internal;
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

    internal static DuskMainAssets DuskMain { get; private set; } = null!;
    internal static DawnLibMainNetworkAssets DawnLibMainNetwork { get; private set; } = null!;
    internal static DawnLibMainVanillaAssets DawnLibMainVanilla { get; private set; } = null!;
    internal static DuskPlugin Instance { get; private set; } = null!;

    private void Awake()
    {
        Instance = this;
        Logger = base.Logger;
        PersistentData = this.GetPersistentDataContainer();
        Logger.LogInfo("Doing patches");

        AchievementRegistrationPatch.Init();
        EntityReplacementRegistrationPatch.Init();
        DuskSaveIntegration.Init();
        CommitKeyToSave.Init();
        if (!DawnConfig.VanillaCompatibility.Value)
        {
            NetworkerPatch.Init();
            VehicleRegistrationPatch.Init();
        }

        Logger.LogInfo("Loading assets");

        if (!DawnConfig.VanillaCompatibility.Value)
        {
            DawnLibMainNetwork = new DawnLibMainNetworkAssets(AssetBundleUtils.LoadBundle(Assembly.GetExecutingAssembly(), "dawnlibmainnetwork"));
        }
        DuskMain = new DuskMainAssets(AssetBundleUtils.LoadBundle(Assembly.GetExecutingAssembly(), "dusklibmain"));
        DawnLibMainVanilla = new DawnLibMainVanillaAssets(AssetBundleUtils.LoadBundle(Assembly.GetExecutingAssembly(), "dawnlibmainvanilla"));
        /*
         * todo: awful hack.
         * because dawn doesn't actually load any assetbundles,
         * and dusk is currently in charge of the main bundles,
         * we have to manually set this field in MoonRegistrationHandler as a workaround
         */
        MoonRegistrationHandler.RouteProgressUIPrefab = DawnLibMainVanilla.RouteProgressUIPrefab;

        Logger.LogInfo("Registering auto DuskMods!");
        AutoDuskModHandler.AutoRegisterMods();
        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    internal class DawnLibMainNetworkAssets(AssetBundle bundle) : AssetBundleLoader<DawnLibMainNetworkAssets>(bundle)
    {
        [LoadFromBundle("DawnLibNetworker.prefab")]
        public GameObject NetworkerPrefab { get; private set; } = null!;
    }

    internal class DuskMainAssets(AssetBundle bundle) : AssetBundleLoader<DuskMainAssets>(bundle)
    {
        [LoadFromBundle("AchievementUICanvas.prefab")]
        public GameObject AchievementUICanvasPrefab { get; private set; } = null!;

        [LoadFromBundle("AchievementGetUICanvas.prefab")]
        public GameObject AchievementGetUICanvasPrefab { get; private set; } = null!;
    }

    internal class DawnLibMainVanillaAssets(AssetBundle bundle) : AssetBundleLoader<DawnLibMainVanillaAssets>(bundle)
    {
        [LoadFromBundle("DawnLibRouteProgressUI.prefab")]
        public GameObject RouteProgressUIPrefab { get; private set; } = null!;
    }
}