using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using Dawn.Utils;
using LethalConfig;
using LethalConfig.ConfigItems;
using On.LethalConfig.AutoConfig;
using UnityEngine;

namespace Dawn.Internal;
static class LethalConfigCompat
{
    internal const string VERSION = "1.4.6";

    private static ConfigFile _dummyConfig;
    private static readonly FieldInfo _typedValueField = typeof(ConfigEntry<>).GetField("_typedValue", BindingFlags.Instance | BindingFlags.NonPublic);
    public static bool Enabled => Chainloader.PluginInfos.ContainsKey(PluginInfo.Guid) && CodeRebirthLibConfig.LethalConfigCompatibility.ShouldRunCompatibility(VERSION, Chainloader.PluginInfos[PluginInfo.Guid].Metadata.Version);


    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void Init()
    {
        _dummyConfig = new ConfigFile(Path.Combine(Application.persistentDataPath, "coderebirthlib.dummy.youshouldneverseethis.cfg"), false);
        _dummyConfig.SaveOnConfigSet = false;

        AutoConfigGenerator.GenerateConfigForEntry += ExtendGenerateConfigForEntry;
    }

    private static BaseConfigItem? ExtendGenerateConfigForEntry(AutoConfigGenerator.orig_GenerateConfigForEntry orig, ConfigEntryBase configEntryBase)
    {
        try
        {
            Debuggers.LethalConfig?.Log("On.GenerateConfigForEntry");
            BaseConfigItem result = orig(configEntryBase);
            Debuggers.LethalConfig?.Log($"result is null? {result == null}");

            if (result != null) return result;

            // Check if BepInEx still can actually support this type
            if (!TomlTypeConverter.CanConvert(configEntryBase.SettingType)) return null;
            Debuggers.LethalConfig?.Log($"toml type converter can actually support: {configEntryBase.SettingType}");

            // Create a poxy entry to spoof it as a string.
            ConfigEntry<string> proxyEntry = _dummyConfig.Bind(
                configEntryBase.Definition.Section,
                configEntryBase.Definition.Key,
                TomlTypeConverter.ConvertToString(configEntryBase.BoxedValue, configEntryBase.SettingType),
                configEntryBase.Description.Description
            );

            proxyEntry.SettingChanged += (sender, args) => { configEntryBase.BoxedValue = TomlTypeConverter.ConvertToValue(proxyEntry.Value, configEntryBase.SettingType); };
            _dummyConfig.SettingChanged += (sender, args) =>
            {
                if (args.ChangedSetting == configEntryBase)
                {
                    // use reflection to set the _typedValue directly, so it doesn't fire the SettingChanged event (which we register to above)
                    _typedValueField.SetValue(proxyEntry, TomlTypeConverter.ConvertToString(configEntryBase.BoxedValue, configEntryBase.SettingType));
                }
            };

            return orig(proxyEntry);
        }
        catch (Exception exception)
        {
            CodeRebirthLibPlugin.Logger.LogError($"Caught actual LethalConfig error: \n{exception}");
            return null;
        }
    }
}