using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace Dusk;
// todo: look over this again and see how much can become toml types?
public class ConfigManager(ConfigFile file)
{
    public ConfigFile File { get; } = file;

    public ConfigContext CreateConfigSection(string header)
    {
        return new ConfigContext(File, header);
    }

    public ConfigContext CreateConfigSectionForBundleData(AssetBundleData data)
    {
        return CreateConfigSection(data.configName + " Options");
    }

    public static Dictionary<string, string> ParseNamespacedKeyWithCurves(string configString)
    {
        Dictionary<string, string> spawnRateByMoonName = new();
        foreach (string configEntry in configString.Split('|', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()))
        {
            string[] entryParts = configEntry.Split('-').Select(s => s.Trim()).ToArray();

            if (entryParts.Length != 2)
                continue;

            string name = entryParts[0].ToLowerInvariant();
            spawnRateByMoonName[name] = entryParts[1];
        }
        return spawnRateByMoonName;
    }

    // todo: mark obsolete?
    public static AnimationCurve ParseCurve(string keyValuePairs)
    {
        return TomlTypeConverter.ConvertToValue<AnimationCurve>(keyValuePairs);
    }

    public static string ParseString(AnimationCurve animationCurve)
    {
        return TomlTypeConverter.ConvertToString(animationCurve, typeof(AnimationCurve));
    }

    internal static ConfigFile GenerateConfigFile(BepInPlugin plugin)
    {
        return new ConfigFile(Utility.CombinePaths(Paths.ConfigPath, plugin.GUID + ".cfg"), false, plugin);
    }

    internal static ConfigFile GenerateConfigFile(string configFileName)
    {
        return new ConfigFile(Utility.CombinePaths(Paths.ConfigPath, configFileName), false);
    }
}