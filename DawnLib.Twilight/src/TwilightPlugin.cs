using BepInEx;
using BepInEx.Logging;
using Dawn;
using Dawn.Internal;
using Dawn.Utils;
using Dusk;
using Twilight.Utils;

namespace Twilight;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(DawnLib.PLUGIN_GUID)]
[BepInDependency(DuskMod.PLUGIN_GUID)]
public class TwilightPlugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static PersistentDataContainer PersistentData { get; private set; } = null!;

    internal static TwilightPlugin Instance { get; private set; } = null!;

    private void Awake()
    {
        Instance = this;
        Logger = base.Logger;
        PersistentData = this.GetPersistentDataContainer();
        Logger.LogInfo("Doing patches");

        if (!DawnConfig.VanillaCompatibility.Value)
        {
            NamespacedSpawnSyncedObject.Init();
        }

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }
}