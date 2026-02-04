using BepInEx;
using BepInEx.Logging;
using loaforcsSoundAPI;

namespace Dawn.Compatibility;
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(DawnLib.PLUGIN_GUID)]
[BepInDependency(SoundAPI.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
public class DawnCompatibilityPlugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger { get; private set; } = null!;

    private void Awake()
    {
        Logger = base.Logger;

        if (SoundAPICompat.Enabled)
        {
            SoundAPICompat.Init();
        }

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }
}