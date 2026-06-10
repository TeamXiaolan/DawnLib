using System.Collections.Generic;
using UnityEngine;

namespace Dawn;

public sealed class MoonCurveSource : IWeightModifierSource<AnimationCurve?>
{
    private readonly NamespacedKey<DawnMoonInfo> _key;
    private readonly AnimationCurve _curve;

    public MoonCurveSource(NamespacedKey<DawnMoonInfo> key, AnimationCurve curve)
    {
        _key = key;
        _curve = curve;
    }

    public void Build(WeightBuildContext context, List<IWeightModifier<AnimationCurve?>> modifiers)
    {
        modifiers.Add(new MoonCurveModifier(_key, _curve));
    }
}