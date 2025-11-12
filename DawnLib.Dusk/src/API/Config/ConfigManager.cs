using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace Dusk;
// todo: look over this again and see how much can become toml types?
public class ConfigManager(ConfigFile file)
{
    private static readonly Regex ConfigCleanerRegex = new(@"[\n\t""`\[\]']");
    internal static string CleanStringForConfig(string input)
    {
        // The regex pattern matches: newline, tab, double quote, backtick, apostrophe, [ or ].
        return ConfigCleanerRegex.Replace(input, string.Empty);
    }

    public ConfigContext CreateConfigSection(string header)
    {
        return new ConfigContext(file, header);
    }

    public ConfigContext CreateConfigSectionForBundleData(AssetBundleData data)
    {
        return CreateConfigSection(data.configName + " Options");
    }

    public ConfigEntryBase CreateDynamicConfig(DuskDynamicConfig configDefinition, ConfigContext context)
    {
        ConfigEntryBase Bind<T>(T defaultValue)
        {
            return context.Bind(configDefinition.settingName, configDefinition.Description, defaultValue);
        }

        return configDefinition.DynamicConfigType switch
        {
            DuskDynamicConfigType.String => Bind(configDefinition.defaultString),
            DuskDynamicConfigType.Int => Bind(configDefinition.defaultInt),
            DuskDynamicConfigType.Bool => Bind(configDefinition.defaultBool),
            DuskDynamicConfigType.Float => Bind(configDefinition.defaultFloat),
            DuskDynamicConfigType.BoundedRange => Bind(configDefinition.defaultBoundedRange),
            DuskDynamicConfigType.AnimationCurve => Bind(configDefinition.defaultAnimationCurve),
            _ => throw new ArgumentOutOfRangeException($"DynamicConfigType of '{configDefinition.DynamicConfigType}' is not yet internally implemented!!"),
        };
    }

    public static Dictionary<string, string> ParseLevelNameWithCurves(string configMoonRarity)
    {
        Dictionary<string, string> spawnRateByMoonName = new();
        foreach (string entry in configMoonRarity.Split('|', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()))
        {
            string[] entryParts = entry.Split('-').Select(s => s.Trim()).ToArray();

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
}