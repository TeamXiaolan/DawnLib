using System.Collections.Generic;
using System.Linq;
using Dawn;
using Dawn.Internal;
using UnityEngine;

namespace Dusk;

public class MapObjectSpawnMechanics : IContextualProvider<AnimationCurve?, DawnMoonInfo>
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
            CurvesByMoonOrTagName[moonName] = ConfigManager.ParseCurve(value);
        }
    }

    public Dictionary<string, AnimationCurve> CurvesByMoonOrTagName { get; } = new();

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
        if (CurvesByMoonOrTagName.TryGetValue(actualLevelName, out AnimationCurve curve))
        {
            return curve;
        }

        if (level.TryGetDawnInfo(out DawnMoonInfo? moonInfo))
        {
            List<AnimationCurve> tagCurveCandidates = new();
            foreach ((string tagName, AnimationCurve tagCurve) in CurvesByMoonOrTagName)
            {
                if (!NamespacedKey.TryParse(tagName, out NamespacedKey? key))
                    continue;

                if (!moonInfo.HasTag(key))
                    continue;

                tagCurveCandidates.Add(tagCurve);
            }

            if (tagCurveCandidates.Count > 0)
            {
                List<Keyframe> averagedKeyframes = new();
                for (float i = 0; i < 1; i += 0.01f)
                {
                    List<float> curveEvals = new();
                    foreach (AnimationCurve tagCurve in tagCurveCandidates)
                    {
                        curveEvals.Add(tagCurve.Evaluate(i));
                    }

                    float average = curveEvals.Average();
                    averagedKeyframes.Add(new Keyframe(i, average));
                }

                return new AnimationCurve(averagedKeyframes.ToArray());
            }
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
        Debuggers.MapObjects?.Log($"Failed to find curve for level: {level}");
        return AnimationCurve.Constant(0, 1, 0); // Default case if no curve matches
    }

    public AnimationCurve? Provide(DawnMoonInfo info)
    {
        return CurveFunction(info.Level);
    }
}