using System.Collections.Generic;
using System.Linq;
using Dawn;
using Dawn.Internal;
using UnityEngine;

namespace Dusk;

public class MapObjectSpawnMechanicsModifier : IWeightModifier<AnimationCurve?>
{
    private static readonly AnimationCurve ZeroCurve = AnimationCurve.Constant(0, 1, 0);

    private readonly Dictionary<NamespacedKey, AnimationCurve> _curvesByMoonOrTagName;
    private readonly Dictionary<NamespacedKey, AnimationCurve> _curvesByInteriorOrTagName;
    private readonly bool _prioritiseMoons;

    public MapObjectSpawnMechanicsModifier(Dictionary<NamespacedKey, AnimationCurve> curvesByMoonOrTagName, Dictionary<NamespacedKey, AnimationCurve> curvesByInteriorOrTagName, bool prioritiseMoons)
    {
        _curvesByMoonOrTagName = curvesByMoonOrTagName;
        _curvesByInteriorOrTagName = curvesByInteriorOrTagName;
        _prioritiseMoons = prioritiseMoons;
    }

    public NamespacedKey Key => DuskKeys.MapObjectSpawnMechanics;

    public WeightModifierPhase Phase => WeightModifierPhase.Override;

    public int Priority => 0;

    public bool CanApply(WeightContext context)
    {
        return context.Moon != null;
    }

    public void Apply(ref AnimationCurve? value, WeightContext context)
    {
        value = GetCurve(context);
    }

    private AnimationCurve GetCurve(WeightContext context)
    {
        DawnMoonInfo? moonInfo = context.Moon;
        if (moonInfo == null || moonInfo.Level == null)
            return ZeroCurve;

        DawnDungeonInfo? dungeonInfo = context.Dungeon;

        if (_prioritiseMoons && _curvesByMoonOrTagName.TryGetValue(moonInfo.Key, out AnimationCurve curve))
        {
            return curve;
        }

        if (dungeonInfo?.DungeonFlow != null && _curvesByInteriorOrTagName.TryGetValue(dungeonInfo.Key, out curve))
        {
            return curve;
        }

        if (!_prioritiseMoons && _curvesByMoonOrTagName.TryGetValue(moonInfo.Key, out curve))
        {
            return curve;
        }

        if (dungeonInfo?.DungeonFlow == null)
            return ZeroCurve;

        List<AnimationCurve> tagCurveCandidates = GetTagCurveCandidates(moonInfo, dungeonInfo);

        if (tagCurveCandidates.Count > 0)
        {
            return AverageCurves(tagCurveCandidates);
        }

        Debuggers.MapObjects?.Log($"Failed to find curve for level: {moonInfo.Level}");
        return ZeroCurve;
    }

    private List<AnimationCurve> GetTagCurveCandidates(DawnMoonInfo moonInfo, DawnDungeonInfo dungeonInfo)
    {
        List<AnimationCurve> candidates = new();
        if (_prioritiseMoons)
        {
            foreach ((NamespacedKey tagName, AnimationCurve tagCurve) in _curvesByMoonOrTagName)
            {
                if (!moonInfo.HasTag(tagName))
                    continue;

                candidates.Add(tagCurve);
            }

            return candidates;
        }

        foreach ((NamespacedKey tagName, AnimationCurve tagCurve) in _curvesByInteriorOrTagName)
        {
            if (!dungeonInfo.HasTag(tagName))
                continue;

            candidates.Add(tagCurve);
        }

        return candidates;
    }

    private static AnimationCurve AverageCurves(List<AnimationCurve> curves)
    {
        List<Keyframe> averagedKeyframes = new();

        for (float i = 0; i < 1; i += 0.01f)
        {
            float average = curves.Average(curve => curve.Evaluate(i));
            averagedKeyframes.Add(new Keyframe(i, average));
        }

        return new AnimationCurve(averagedKeyframes.ToArray());
    }
}