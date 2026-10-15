using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using Dawn;
using Dawn.Internal;
using Dawn.Utils;
using Dusk;
using Nightfall.Internal;
using Nightfall.Weights;
using UnityEngine;

namespace Nightfall;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(DawnLib.PLUGIN_GUID)]
[BepInDependency(DuskMod.PLUGIN_GUID)]
public class NightfallPlugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static PersistentDataContainer PersistentData { get; private set; } = null!;

    internal static NightfallMainNetworkAssets NightfallMainNetwork { get; private set; } = null!;
    internal static NightfallPlugin Instance { get; private set; } = null!;

    private void Awake()
    {
        Instance = this;
        Logger = base.Logger;
        PersistentData = this.GetPersistentDataContainer();

        DawnLib.Weights.AddContextContributor(new ItemInfoWeightContextContributor());
        Logger.LogInfo("Doing patches");

        EntityReplacementRegistrationPatch.Init();
        if (!DawnConfig.VanillaCompatibility.Value)
        {
            NetworkerPatch.Init();
        }

        Logger.LogInfo("Loading assets");

        if (!DawnConfig.VanillaCompatibility.Value)
        {
            NightfallMainNetwork = new NightfallMainNetworkAssets(AssetBundleUtils.LoadBundle(Assembly.GetExecutingAssembly(), "nightfallmainnetwork"));
        }

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    internal class NightfallMainNetworkAssets(AssetBundle bundle) : AssetBundleLoader<NightfallMainNetworkAssets>(bundle)
    {
        [LoadFromBundle("NightfallNetworker.prefab")]
        public GameObject NetworkerPrefab { get; private set; } = null!;
    }
}