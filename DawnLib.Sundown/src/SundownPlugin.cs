using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Dawn;
using Dawn.Utils;
using Dusk;

namespace Sundown;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(DawnLib.PLUGIN_GUID)]
[BepInDependency(DuskMod.PLUGIN_GUID)]
public class SundownPlugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static PersistentDataContainer PersistentData { get; private set; } = null!;

    internal static SundownPlugin Instance { get; private set; } = null!;

    private static Dictionary<DuskContentDefinition, List<SundownConfigDefinition>> _configDefinitions = new();

    private void Awake()
    {
        Instance = this;
        Logger = base.Logger;
        PersistentData = this.GetPersistentDataContainer();

        DuskModBundleLoader.PreContentRegistration += (context, allDefinitions) =>
        {
            foreach (DuskContentDefinition definition in allDefinitions)
            {
                if (definition is not SundownConfigDefinition configDefinition)
                {
                    continue;
                }

                if (configDefinition.ContentReference == null)
                {
                    continue;
                }

                if (!_configDefinitions.ContainsKey(configDefinition.ContentReference))
                {
                    _configDefinitions[configDefinition.ContentReference] = new List<SundownConfigDefinition>();
                }
                _configDefinitions[configDefinition.ContentReference].Add(configDefinition);
            }
        };

        DuskModBundleLoader.OnContentRegistered += (context, definition) =>
        {
            if (_configDefinitions.TryGetValue(definition, out List<SundownConfigDefinition>? configs))
            {
                foreach (SundownConfigDefinition config in configs)
                {
                    config.ManuallyRegister(context);
                }
            }

            DuskBaseConfig? BaseConfig = definition.BaseConfig;
            if (BaseConfig != null)
            {
                foreach (ConfigEntryBase entry in BaseConfig.ConfigEntries())
                {
                    context.Mod.configEntries.Add(entry);
                }
            }
        };

        DuskModBundleLoader.PostContentRegistration += (context, definition) =>
        {
            _configDefinitions.Clear();
        };

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }
}