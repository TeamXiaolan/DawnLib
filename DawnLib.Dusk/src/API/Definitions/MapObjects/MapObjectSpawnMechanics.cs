using System;
using System.Collections.Generic;
using Dawn;
using Dawn.Internal;
using UnityEngine;

namespace Dusk;

public sealed class MapObjectSpawnMechanics : WeightModifierSource<AnimationCurve?>
{
    private readonly Func<string> _getMoonConfigString;
    private readonly Func<string> _getInteriorConfigString;
    private readonly Func<bool> _getPrioritiseMoons;

    public MapObjectSpawnMechanics(Func<string> getMoonConfigString, Func<string> getInteriorConfigString, Func<bool> getPrioritiseMoons)
    {
        _getMoonConfigString = getMoonConfigString;
        _getInteriorConfigString = getInteriorConfigString;
        _getPrioritiseMoons = getPrioritiseMoons;
    }

    public override void Build(WeightBuildContext context, List<IWeightModifier<AnimationCurve?>> modifiers)
    {
        Debuggers.Weights?.Log($"Building MapObjectSpawnMechanics with moon config '{_getMoonConfigString()}");
        using NamespacedKeyResolver<DawnMoonInfo> moonResolver = new(context.Moons.Values);
        Dictionary<NamespacedKey, AnimationCurve> moonCurves = ResolveCurves(_getMoonConfigString(), moonResolver);

        Debuggers.Weights?.Log($"Building MapObjectSpawnMechanics with interior config '{_getInteriorConfigString()}'");
        using NamespacedKeyResolver<DawnDungeonInfo> interiorResolver = new(context.Dungeons.Values);
        Dictionary<NamespacedKey, AnimationCurve> interiorCurves = ResolveCurves(_getInteriorConfigString(), interiorResolver);

        modifiers.Add(new MapObjectSpawnMechanicsModifier(moonCurves, interiorCurves, _getPrioritiseMoons()));
    }

    private static Dictionary<NamespacedKey, AnimationCurve> ResolveCurves<T>(string configString, NamespacedKeyResolver<T> resolver) where T : INamespaced<T>
    {
        Dictionary<NamespacedKey, AnimationCurve> resolvedCurves = new();
        Dictionary<string, string> parsedCurves = ConfigManager.ParseNamespacedKeyWithCurves(configString);
        foreach ((string keyInput, string curveInput) in parsedCurves)
        {
            if (string.IsNullOrWhiteSpace(keyInput))
                continue;

            if (!resolver.TryResolve(keyInput.Trim(), out NamespacedKey<T>? resolvedKey) || resolvedKey == null)
            {
                continue;
            }

            resolvedCurves[resolvedKey] = ConfigManager.ParseCurve(curveInput);
        }

        return resolvedCurves;
    }
}