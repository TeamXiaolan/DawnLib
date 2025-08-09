using System;
using BepInEx;
using BepInEx.Logging;

namespace CodeRebirthLib;
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class CodeRebirthLibPlugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger { get; private set; } = null!;

    private void Awake()
    {
        Logger = base.Logger;
        ItemRegistrationHandler.Init();
    }
}