using System.Collections.Generic;
using System.Linq;
using Dawn;
using Dawn.Internal;
using UnityEngine;

namespace Dusk;

public class MapObjectSpawnMechanics : IWeightModifierSource<AnimationCurve?>
{
    private static readonly AnimationCurve ZeroCurve = AnimationCurve.Constant(0, 1, 0);

    private readonly Dictionary<string, AnimationCurve> _unresolvedMoonCurves = new();
    private readonly Dictionary<string, AnimationCurve> _unresolvedInteriorCurves = new();

    public Dictionary<NamespacedKey, AnimationCurve> CurvesByMoonOrTagName { get; } = new();
    public Dictionary<NamespacedKey, AnimationCurve> CurvesByInteriorOrTagName { get; } = new();

    public bool PrioritiseMoons { get; }

    public MapObjectSpawnMechanics(string moonConfigString, string interiorConfigString, bool prioritiseMoons = true)
    {
        AddUnresolvedCurves(moonConfigString, _unresolvedMoonCurves);
        AddUnresolvedCurves(interiorConfigString, _unresolvedInteriorCurves);

        PrioritiseMoons = prioritiseMoons;

        LethalContent.Moons.AfterTaggingWithContext += ResolveMoonCurves;
        LethalContent.Dungeons.AfterTaggingWithContext += ResolveInteriorCurves;
    }

    private static void AddUnresolvedCurves(string configString, Dictionary<string, AnimationCurve> target)
    {
        Dictionary<string, string> parsedCurves = ConfigManager.ParseNamespacedKeyWithCurves(configString);

        foreach ((string keyInput, string curveInput) in parsedCurves)
        {
            if (string.IsNullOrWhiteSpace(keyInput))
            {
                continue;
            }

            target[keyInput.Trim()] = ConfigManager.ParseCurve(curveInput);
        }
    }

    private void ResolveMoonCurves(NamespacedKeyResolver<DawnMoonInfo> resolver)
    {
        ResolveCurves(resolver, _unresolvedMoonCurves, CurvesByMoonOrTagName);
    }

    private void ResolveInteriorCurves(NamespacedKeyResolver<DawnDungeonInfo> resolver)
    {
        ResolveCurves(resolver, _unresolvedInteriorCurves, CurvesByInteriorOrTagName);
    }

    private static void ResolveCurves<T>(NamespacedKeyResolver<T> resolver, Dictionary<string, AnimationCurve> unresolvedCurves, Dictionary<NamespacedKey, AnimationCurve> resolvedCurves) where T : INamespaced<T>
    {
        resolvedCurves.Clear();

        foreach ((string keyInput, AnimationCurve curve) in unresolvedCurves)
        {
            if (!resolver.TryResolve(keyInput, out NamespacedKey<T>? resolvedKey) || resolvedKey == null)
            {
                Debuggers.MapObjects?.Log($"Could not resolve key input '{keyInput}'.");
                continue;
            }

            resolvedCurves[resolvedKey] = curve;
        }
    }

    public void Build(WeightBuildContext context, List<IWeightModifier<AnimationCurve?>> modifiers)
    {
        modifiers.Add(new MapObjectSpawnMechanicsModifier(this));
    }

    public AnimationCurve GetCurve(WeightContext context)
    {
        DawnMoonInfo? moonInfo = context.Moon;

        if (moonInfo == null || moonInfo.Level == null)
        {
            return ZeroCurve;
        }

        DawnDungeonInfo? dungeonInfo = context.Dungeon;

        if (PrioritiseMoons && CurvesByMoonOrTagName.TryGetValue(moonInfo.Key, out AnimationCurve curve))
        {
            return curve;
        }
        else if (dungeonInfo != null && dungeonInfo.DungeonFlow != null && CurvesByInteriorOrTagName.TryGetValue(dungeonInfo.Key, out curve))
        {
            return curve;
        }
        else if (!PrioritiseMoons && CurvesByMoonOrTagName.TryGetValue(moonInfo.Key, out curve))
        {
            return curve;
        }

        if (dungeonInfo == null || dungeonInfo.DungeonFlow == null)
        {
            return ZeroCurve;
        }

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

        if (PrioritiseMoons)
        {
            foreach ((NamespacedKey tagName, AnimationCurve tagCurve) in CurvesByMoonOrTagName)
            {
                if (!moonInfo.HasTag(tagName))
                {
                    continue;
                }

                candidates.Add(tagCurve);
            }

            return candidates;
        }

        foreach ((NamespacedKey tagName, AnimationCurve tagCurve) in CurvesByInteriorOrTagName)
        {
            if (!dungeonInfo.HasTag(tagName))
            {
                continue;
            }

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