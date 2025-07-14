using System;
using System.Collections.Generic;
using System.Linq;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.ModCompats;
using LethalLib.Modules;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement.MapObjects;
public class MapObjectSpawnMechanics
{

    public MapObjectSpawnMechanics(string configString)
    {
        (Dictionary<Levels.LevelTypes, string> spawnRateByLevelType, Dictionary<string, string> spawnRateByCustomLevelType) = ConfigManager.ParseLevelTypesWithCurves(configString);

        foreach ((Levels.LevelTypes levelType, string value) in spawnRateByLevelType)
        {
            AnimationCurve parsed = ConfigManager.ParseCurve(value);
            CurvesByLevelType[levelType] = parsed;

            switch (levelType)
            {
                case Levels.LevelTypes.Vanilla:
                    Vanilla = parsed;
                    break;
                case Levels.LevelTypes.Modded:
                    Modded = parsed;
                    break;
                case Levels.LevelTypes.All:
                    All = parsed;
                    break;
            }
        }
        foreach ((string moonName, string value) in spawnRateByCustomLevelType)
        {
            CurvesByCustomLevelType[moonName] = ConfigManager.ParseCurve(value);
        }
    }
    public Dictionary<Levels.LevelTypes, AnimationCurve> CurvesByLevelType { get; } = new();
    public Dictionary<string, AnimationCurve> CurvesByCustomLevelType { get; } = new();

    public AnimationCurve? All { get; }
    public AnimationCurve? Vanilla { get; }
    public AnimationCurve? Modded { get; }

    public string[] LevelOverrides => CurvesByCustomLevelType.Keys.Select(it => it.ToLowerInvariant()).ToArray();

    public AnimationCurve CurveFunction(SelectableLevel level)
    {
        if (level == null)
            return AnimationCurve.Linear(0, 0, 1, 0);

        string actualLevelName = Levels.Compatibility.GetLLLNameOfLevel(level.name);
        bool isValidLevelType = Enum.TryParse(actualLevelName, true, out Levels.LevelTypes levelType);
        bool isVanilla = isValidLevelType && levelType != Levels.LevelTypes.Modded;
        CodeRebirthLibPlugin.ExtendedLogging($"Actual level name: {actualLevelName} | LevelType: {levelType} | isValidLevelType: {isValidLevelType} | isVanilla: {isVanilla}");
        if (CurvesByLevelType.TryGetValue(levelType, out AnimationCurve curve))
        {
            return curve;
        }
        if (CurvesByCustomLevelType.TryGetValue(actualLevelName, out curve))
        {
            return curve;
        }
        if (LLLCompatibility.Enabled && LLLCompatibility.TryGetCurveDictAndLevelTag(CurvesByCustomLevelType, level, out string tagName) && CurvesByCustomLevelType.TryGetValue(tagName, out curve))
        {
            CodeRebirthLibPlugin.ExtendedLogging("registering a mapobject through a tag, nice.");
            return curve;
        }
        if (isVanilla && Vanilla != null)
        {
            return Vanilla;
        }
        if (Modded != null)
        {
            return Modded;
        }
        if (All != null)
        {
            return All;
        }
        CodeRebirthLibPlugin.ExtendedLogging($"Failed to find curve for level: {level}");
        return AnimationCurve.Linear(0, 0, 1, 0); // Default case if no curve matches
    }
}