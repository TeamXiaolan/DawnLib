using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using CodeRebirthLib.Data;
using HarmonyLib;
using LethalConfig;
using LethalConfig.AutoConfig;
using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;
using LethalConfig.MonoBehaviours.Components;
using LethalConfig.MonoBehaviours.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using AutoConfigGenerator = On.LethalConfig.AutoConfig.AutoConfigGenerator;
using Object = UnityEngine.Object;

namespace CodeRebirthLib.Patches;

/*
 * LethalConfig is a little dumb idiot and doesn't default to a string text input if it encouters a type it doesn't know.
 * I would make a pull request to it, but i am not setting up thunderkit (not to mention it looks abandoned)
 */
static class LethalConfigPatch
{
    internal const string VERSION = "1.4.6";
    
    private static ConfigFile _dummyConfig;

    private static FieldInfo _typedValueField = typeof(ConfigEntry<>).GetField("_typedValue", BindingFlags.Instance | BindingFlags.NonPublic);
    
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static void Patch()
    {
        if(!CodeRebirthLibConfig.LethalConfigCompatibility.ShouldRunCompatibility(VERSION, Chainloader.PluginInfos["ainavt.lc.lethalconfig"].Metadata.Version))
            return;
        
        _dummyConfig = new ConfigFile(Path.Combine(Application.persistentDataPath, "coderebirthlib.dummy.youshouldneverseethis.cfg"), false);
        _dummyConfig.SaveOnConfigSet = false;
        
        On.LethalConfig.AutoConfig.AutoConfigGenerator.GenerateConfigForEntry += ExtendGenerateConfigForEntry;
    }
    private static BaseConfigItem ExtendGenerateConfigForEntry(AutoConfigGenerator.orig_GenerateConfigForEntry orig, ConfigEntryBase configEntryBase)
    {
        try
        {
            CodeRebirthLibPlugin.ExtendedLogging("[LethalConfigPatch] On.GenerateConfigForEntry");
            BaseConfigItem result = orig(configEntryBase);
            CodeRebirthLibPlugin.ExtendedLogging($"[LethalConfigPatch] result is null? {result == null}");

            if (result != null) return result;

            // Check if BepInEx still can actually support this type
            if (!TomlTypeConverter.CanConvert(configEntryBase.SettingType)) return result;
            CodeRebirthLibPlugin.ExtendedLogging($"[LethalConfigPatch] toml type converter can actually support: {configEntryBase.SettingType}");
            
            // Create a poxy entry to spoof it as a string.
            ConfigEntry<string> proxyEntry = _dummyConfig.Bind(configEntryBase.Definition, TomlTypeConverter.ConvertToString(configEntryBase.BoxedValue, configEntryBase.SettingType), configEntryBase.Description);

            proxyEntry.SettingChanged += (sender, args) =>
            {
                configEntryBase.BoxedValue = TomlTypeConverter.ConvertToValue(proxyEntry.Value, configEntryBase.SettingType);
            };
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