using System;
using System.Collections.Generic;
using System.Linq;
using Dawn.Internal;
using UnityEngine;

namespace Dawn;

public class LevelListBaseCurveSource<TEntry> : WeightModifierSource<AnimationCurve?>
{
    private readonly Func<SelectableLevel, IEnumerable<TEntry>> _getEntries;
    private readonly Func<TEntry, bool> _matches;
    private readonly Func<TEntry, AnimationCurve?> _getCurve;

    public LevelListBaseCurveSource(Func<SelectableLevel, IEnumerable<TEntry>> getEntries, Func<TEntry, bool> matches, Func<TEntry, AnimationCurve?> getCurve)
    {
        _getEntries = getEntries;
        _matches = matches;
        _getCurve = getCurve;
    }

    public override void Build(WeightBuildContext context, List<IWeightModifier<AnimationCurve?>> modifiers)
    {
        foreach (DawnMoonInfo moonInfo in context.Moons.Values)
        {
            SelectableLevel level = moonInfo.Level;
            TEntry? entry = _getEntries(level).FirstOrDefault(_matches);
            if (entry == null)
                continue;

            AnimationCurve? curve = _getCurve(entry);
            if (curve == null)
                continue;

            Debuggers.MapObjects?.Log($"Adding curve {curve} for {entry} on level {level.PlanetName}");
            modifiers.Add(new MoonBaseCurveModifier(moonInfo.TypedKey, curve));
        }
    }
}