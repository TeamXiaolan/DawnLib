using System;
using System.Collections.Generic;
using System.Linq;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.Exceptions;
using LethalLib.Modules;
using UnityEngine;

namespace CodeRebirthLib.ContentManagement.MapObjects;
public class MapObjectSpawnMechanics
{
    public Dictionary<Levels.LevelTypes, AnimationCurve> CurvesByLevelType { get; } = new();
    public Dictionary<string, AnimationCurve> CurvesByCustomLevelType { get; } = new();

    public AnimationCurve? All { get; private set; } = null;
    public AnimationCurve? Vanilla { get; private set; } = null;
    public AnimationCurve? Modded { get; private set; } = null;

    public string[] LevelOverrides => CurvesByCustomLevelType.Keys.Select(it => it.ToLowerInvariant()).ToArray();
    
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
    
    public AnimationCurve CurveFunction(SelectableLevel level)
    {
        if (level == null)
            return AnimationCurve.Linear(0, 0, 1, 0);

        string actualLevelName = Levels.Compatibility.GetLLLNameOfLevel(level.sceneName);

        bool isValidLevelType = Enum.TryParse(actualLevelName, true, out Levels.LevelTypes levelType);
        bool isVanilla = isValidLevelType && levelType != Levels.LevelTypes.Modded;

        if (CurvesByLevelType.TryGetValue(levelType, out AnimationCurve curve))
        {
            return curve;
        }
        else if (CurvesByCustomLevelType.TryGetValue(actualLevelName, out curve))
        {
            return curve;
        }
        /*else if (TryGetCurveDictAndLevelTag(curvesByCustomLevelType, level, out string tagName) && curvesByCustomLevelType.TryGetValue(tagName, out curve))
        {
            CodeRebirthLibPlugin.ExtendedLogging($"registering a mapobject through a tag, nice.");
            return curve;
        }*/
        else if (isVanilla && Vanilla != null)
        {
            return Vanilla;
        }
        else if (Modded != null)
        {
            return Modded;
        }
        else if (All != null)
        {
            return All;
        }
        CodeRebirthLibPlugin.ExtendedLogging($"Failed to find curve for level: {level}");
        return AnimationCurve.Linear(0, 0, 1, 0); // Default case if no curve matches
    }
}