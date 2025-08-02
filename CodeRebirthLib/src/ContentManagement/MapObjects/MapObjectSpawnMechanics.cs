using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.ContentManagement.Enemies;
using CodeRebirthLib.ModCompats;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement.MapObjects;

public class MapObjectSpawnMechanics
{

    public MapObjectSpawnMechanics(string configString)
    {
        Dictionary<string, string> spawnRateByMoonName = ConfigManager.ParseLevelNameWithCurves(configString);

        foreach ((string potentialLevelType, string value) in spawnRateByMoonName)
        {
            AnimationCurve parsed = ConfigManager.ParseCurve(value);
            switch (potentialLevelType)
            {
                case "vanilla":
                    VanillaCurve = parsed;
                    break;
                case "modded":
                    ModdedCurve = parsed;
                    break;
                case "all":
                    AllCurve = parsed;
                    break;
            }
        }

        foreach ((string moonName, string value) in spawnRateByMoonName)
        {
            CurvesByMoonName[moonName] = ConfigManager.ParseCurve(value);
        }
    }

    public Dictionary<string, AnimationCurve> CurvesByMoonName { get; } = new();

    public AnimationCurve? AllCurve { get; }
    public AnimationCurve? VanillaCurve { get; }
    public AnimationCurve? ModdedCurve { get; }

    public AnimationCurve CurveFunction(SelectableLevel level)
    {
        if (level == null)
            return AnimationCurve.Linear(0, 0, 1, 0);

        string actualLevelName = GetLLLNameOfLevel(level.name);
        // bool isValidLevelType = Enum.TryParse(actualLevelName, true, out Levels.LevelTypes levelType); // TODO: some way to determine this stuff without LL, probably just using LLL lol
        bool isVanilla = VanillaLevels.IsVanillaLevel(level);
        CodeRebirthLibPlugin.ExtendedLogging($"Actual level name: {actualLevelName} | isVanilla: {isVanilla}");
        if (isVanilla && CurvesByMoonName.TryGetValue(actualLevelName, out AnimationCurve curve))
        {
            return curve;
        }
        if (LLLCompatibility.Enabled && LLLCompatibility.TryGetCurveDictAndLevelTag(CurvesByMoonName, level, out string tagName) && CurvesByMoonName.TryGetValue(tagName, out curve))
        {
            CodeRebirthLibPlugin.ExtendedLogging("registering a mapobject through a tag, nice.");
            return curve;
        }
        if (isVanilla && VanillaCurve != null)
        {
            return VanillaCurve;
        }
        if (ModdedCurve != null)
        {
            return ModdedCurve;
        }
        if (AllCurve != null)
        {
            return AllCurve;
        }
        CodeRebirthLibPlugin.ExtendedLogging($"Failed to find curve for level: {level}");
        return AnimationCurve.Linear(0, 0, 1, 0); // Default case if no curve matches
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
}