using System.Collections.Generic;
using CodeRebirthLib.Internal;
using UnityEngine;

namespace CodeRebirthLib.CRMod;

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
            return AnimationCurve.Constant(0, 1, 0);

        string actualLevelName = ConfigManager.GetLLLNameOfLevel(level.name);
        bool isVanilla = level.ToNamespacedKey().IsVanilla();
        Debuggers.MapObjects?.Log($"Actual level name: {actualLevelName} | isVanilla: {isVanilla}");
        if (CurvesByMoonName.TryGetValue(actualLevelName, out AnimationCurve curve))
        {
            return curve;
        }
        /*if (LLLCompatibility.Enabled && LLLCompatibility.TryGetCurveDictAndLevelTag(CurvesByMoonName, level, out string tagName) && CurvesByMoonName.TryGetValue(tagName, out curve))
        { TODO
            return curve;
        }*/
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
        Debuggers.MapObjects?.Log($"Failed to find curve for level: {level}");
        return AnimationCurve.Constant(0, 1, 0); // Default case if no curve matches
    }
}