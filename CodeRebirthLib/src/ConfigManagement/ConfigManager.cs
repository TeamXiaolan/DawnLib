using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using CodeRebirthLib.ContentManagement;
using LethalLib.Modules;
using UnityEngine.Rendering;

namespace CodeRebirthLib.ConfigManagement;
public class ConfigManager(ConfigFile file)
{
    private static readonly Regex ConfigCleanerRegex = new Regex(@"[\n\t""`\[\]']");
    static string CleanStringForConfig(string input)
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
    
    public static (Dictionary<Levels.LevelTypes, int> spawnRateByLevelType, Dictionary<string, int> spawnRateByCustomLevelType) ParseMoonsWithRarity(string? configMoonRarity)
    {
        Dictionary<Levels.LevelTypes, int> spawnRateByLevelType = new();
        Dictionary<string, int> spawnRateByCustomLevelType = new();
        if (configMoonRarity == null)
        {
            return (spawnRateByLevelType, spawnRateByCustomLevelType);
        }
        foreach (string entry in configMoonRarity.Split(',').Select(s => s.Trim()))
        {
            string[] entryParts = entry.Split(':').Select(s => s.Trim()).ToArray();

            if (entryParts.Length != 2) continue;

            string name = entryParts[0].ToLowerInvariant();

            if (!int.TryParse(entryParts[1], out int spawnrate)) continue;

            if (name == "custom")
            {
                name = "modded";
            }

            if (Enum.TryParse(name, true, out Levels.LevelTypes levelType))
            {
                spawnRateByLevelType[levelType] = spawnrate;
            }
            else
            {
                // Try appending "Level" to the name and re-attempt parsing
                string modifiedName = name + "Level";
                if (Enum.TryParse(modifiedName, true, out levelType))
                {
                    CodeRebirthLibPlugin.ExtendedLogging($"Parsing vanilla level: {name} with spawnrate: {spawnrate}");
                    spawnRateByLevelType[levelType] = spawnrate;
                }
                else
                {
                    CodeRebirthLibPlugin.ExtendedLogging($"Parsing potential modded level or tag: {name} with spawnrate: {spawnrate}");
                    spawnRateByCustomLevelType[name] = spawnrate;
                }
            }
        }
        return (spawnRateByLevelType, spawnRateByCustomLevelType);
    }
}