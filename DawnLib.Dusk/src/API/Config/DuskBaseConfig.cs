using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using Dawn;
using Dawn.Internal;

namespace Dusk;

public abstract class DuskBaseConfig
{
    public DuskBaseConfig(ConfigContext section, string EntityNameReference)
    {
        bool defaultValue = true;
        if (!DawnPlugin.PersistentData.TryGet(DawnKeys.LastVersion, out string? lastLaunchVersion) || Version.Parse(lastLaunchVersion) >= Version.Parse("0.7.8"))
        {
            defaultValue = false;
        }

        AllowEditingConfig = section.Bind($"{EntityNameReference} | Allow Editing Config", $"Whether you're allowed to edit the config entries for {EntityNameReference}.", defaultValue);
    }

    public ConfigEntry<bool> AllowEditingConfig;

    public bool UserAllowedToEdit() => AllowEditingConfig.Value;
    internal static void AssignValueIfNotNull<T>(ConfigEntry<T>? configEntry, T value)
    {
        if (configEntry != null)
        {
            configEntry.Value = value;
        }
    }

    abstract internal List<ConfigEntryBase?> _configEntries { get; }
    public List<ConfigEntryBase> ConfigEntries()
    {
        List<ConfigEntryBase> configEntries = [AllowEditingConfig];
        foreach (ConfigEntryBase? configEntry in _configEntries)
        {
            if (configEntry != null)
            {
                configEntries.Add(configEntry);
            }
        }

        return configEntries;
    }
}