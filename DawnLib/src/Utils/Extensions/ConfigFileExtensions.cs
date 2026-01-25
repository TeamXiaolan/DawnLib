using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;

namespace Dawn.Utils;
public static class ConfigFileExtensions
{
    public static void ClearUnusedEntries(this ConfigFile configFile)
    {
        // Normally, old unused config entries don't get removed, so we do it with this piece of code. Credit to Kittenji.
        PropertyInfo orphanedEntriesProp = configFile.GetType().GetProperty("OrphanedEntries", BindingFlags.NonPublic | BindingFlags.Instance);
        var orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(configFile, null);
        orphanedEntries.Clear(); // Clear orphaned entries (Unbinded/Abandoned entries)
        configFile.Save(); // Save the config file to save these changes
    }

    public static ConfigEntry<T> CleanedBind<T>(this ConfigFile ModConfig, string section, string configItemName, T defaultValue, string ConfigDescription)
    {
        section = section.BepinFriendlyString();
        configItemName = configItemName.BepinFriendlyString();

        return ModConfig.Bind(section, configItemName, defaultValue, new ConfigDescription(ConfigDescription));
    }

    public static ConfigEntry<T> CleanedBind<T>(this ConfigFile ModConfig, string section, string configItemName, T defaultValue, string description, AcceptableValueList<T> acceptableValues = null!) where T : IEquatable<T>
    {
        section = section.BepinFriendlyString();
        configItemName = configItemName.BepinFriendlyString();

        return ModConfig.Bind(section, configItemName, defaultValue, new ConfigDescription(description, acceptableValues));
    }

    public static ConfigEntry<T> CleanedBind<T>(this ConfigFile ModConfig, string section, string configItemName, T defaultValue, string description, T minValue, T maxValue) where T : IComparable
    {
        section = section.BepinFriendlyString();
        configItemName = configItemName.BepinFriendlyString();
        AcceptableValueRange<T> acceptableRange = new(minValue, maxValue);

        return ModConfig.Bind(section, configItemName, defaultValue, new ConfigDescription(description, acceptableRange));
    }
}