using System.Collections.Generic;
using UnityEngine;

namespace Dawn;

public sealed class GlobalCurveSource : IWeightModifierSource<AnimationCurve?>
{
    private readonly AnimationCurve _curve;

    public GlobalCurveSource(AnimationCurve curve)
    {
        _curve = curve;
    }

    public void Build(WeightBuildContext context, List<IWeightModifier<AnimationCurve?>> modifiers)
    {
        modifiers.Add(new GlobalCurveModifier(_curve));
    }
}