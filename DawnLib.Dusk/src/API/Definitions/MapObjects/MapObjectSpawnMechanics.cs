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

        foreach ((string key, string value) in spawnRateByMoonName)
        {
            CurvesByMoonOrTagName[NamespacedKey.ForceParse(key)] = ConfigManager.ParseCurve(value);
        }
    }

    public Dictionary<NamespacedKey, AnimationCurve> CurvesByMoonOrTagName { get; } = new();


    public AnimationCurve CurveFunction(DawnMoonInfo moonInfo)
    {
        if (moonInfo == null || moonInfo.Level == null)
            return AnimationCurve.Constant(0, 1, 0);

        if (CurvesByMoonOrTagName.TryGetValue(moonInfo.Key, out AnimationCurve curve))
        {
            return curve;
        }

        List<AnimationCurve> tagCurveCandidates = new();
        foreach ((NamespacedKey tagName, AnimationCurve tagCurve) in CurvesByMoonOrTagName)
        {
            if (!moonInfo.HasTag(tagName))
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

        Debuggers.MapObjects?.Log($"Failed to find curve for level: {moonInfo.Level}");
        return AnimationCurve.Constant(0, 1, 0);
    }

    public AnimationCurve? Provide(DawnMoonInfo info)
    {
        return CurveFunction(info);
    }
}