using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using CodeRebirthLib.CRMod;

namespace CodeRebirthLib;
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

    public static ConfigContext CreateSection(this ConfigFile file, string header)
    {
        return new ConfigContext(file, header);
    }
}