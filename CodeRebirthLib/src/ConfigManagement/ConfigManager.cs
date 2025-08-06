using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using CodeRebirthLib.AssetManagement;
using CodeRebirthLib.ContentManagement;
using LethalLib.Modules;
using UnityEngine;

namespace CodeRebirthLib.ConfigManagement;
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

    public ConfigEntry<T> CreateGeneralConfig<T>(
        string header,
        string name,
        T DynamicConfigType,
        string Description)
    {
        return file.Bind(CleanStringForConfig(header), CleanStringForConfig(name), DynamicConfigType, Description);
    }

    public ConfigEntryBase CreateDynamicConfig(CRDynamicConfig configDefinition, ConfigContext context)
    {
        ConfigEntryBase Bind<T>(T defaultValue)
        {
            return context.Bind(configDefinition.settingName, configDefinition.Description, defaultValue);
        }

        return configDefinition.DynamicConfigType switch
        {
            CRDynamicConfigType.String => Bind(configDefinition.defaultString),
            CRDynamicConfigType.Int => Bind(configDefinition.defaultInt),
            CRDynamicConfigType.Bool => Bind(configDefinition.defaultBool),
            CRDynamicConfigType.Float => Bind(configDefinition.defaultFloat),
            CRDynamicConfigType.BoundedRange => Bind(configDefinition.defaultBoundedRange),
            CRDynamicConfigType.AnimationCurve => Bind(configDefinition.defaultAnimationCurve),
            _ => throw new ArgumentOutOfRangeException($"DynamicConfigType of '{configDefinition.DynamicConfigType}' is not yet internally implemented!!"),
        };
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

    public static Dictionary<string, string> ParseLevelNameWithCurves(string configMoonRarity)
    {
        Dictionary<string, string> spawnRateByMoonName = new();
        foreach (string entry in configMoonRarity.Split('|').Select(s => s.Trim()))
        {
            string[] entryParts = entry.Split('-').Select(s => s.Trim()).ToArray();

            if (entryParts.Length != 2) continue;

            string name = entryParts[0].ToLowerInvariant();

            if (name == "custom")
            {
                name = "modded";
            }
            // Try appending "Level" to the name and re-attempt parsing
            spawnRateByMoonName[name] = entryParts[1];
            spawnRateByMoonName[name + "level"] = entryParts[1];
        }
        return spawnRateByMoonName;
    }

    // todo: mark obsolete?
    public static AnimationCurve ParseCurve(string keyValuePairs)
    {
        return TomlTypeConverter.ConvertToValue<AnimationCurve>(keyValuePairs);
    }

    private const string illegalCharacters = ".,?!@#$%^&*()_+-=';:'\"";

    private static string GetNumberlessPlanetName(string planetName)
    {
        if (planetName != null)
            return new string(planetName.SkipWhile(c => !char.IsLetter(c)).ToArray());
        else
            return string.Empty;
    }

    private static string StripSpecialCharacters(string input)
    {
        string returnString = string.Empty;

        foreach (char charmander in input)
            if ((!illegalCharacters.ToCharArray().Contains(charmander) && char.IsLetterOrDigit(charmander)) || charmander.ToString() == " ")
                returnString += charmander;

        return returnString;
    }

    internal static string GetLLLNameOfLevel(string levelName)
    {
        // -> 10 Example
        string newName = StripSpecialCharacters(GetNumberlessPlanetName(levelName));
        // -> Example
        if (!newName.EndsWith("Level", true, CultureInfo.InvariantCulture))
            newName += "Level";
        // -> ExampleLevel
        newName = newName.ToLowerInvariant();
        // -> examplelevel
        return newName;
    }

    internal static SelectableLevel? GetLevelWithName(string levelName)
    {
        foreach (var level in LethalContent.Levels.All)
        {
            if (GetLLLNameOfLevel(level.name) != levelName)
                continue;

            return level;
        }
        return null;
    }
}