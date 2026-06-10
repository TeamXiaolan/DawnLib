using UnityEngine;

namespace Dawn;

public sealed class GlobalCurveModifier : IWeightModifier<AnimationCurve?>
{
    private readonly AnimationCurve _curve;

    public GlobalCurveModifier(AnimationCurve curve)
    {
        _curve = curve;
    }

    public NamespacedKey Key => DawnKeys.GlobalCurve;

    public WeightModifierPhase Phase => WeightModifierPhase.Base;

    public int Priority => -1000;

    public bool CanApply(WeightContext context)
    {
        return true;
    }

    public void Apply(ref AnimationCurve? value, WeightContext context)
    {
        value = _curve;
    }
}