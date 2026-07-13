using UnityEngine;

namespace Dawn;

public sealed class MoonBaseCurveModifier : IWeightModifier<AnimationCurve?>
{
    private readonly NamespacedKey<DawnMoonInfo> _moonKey;
    private readonly AnimationCurve _curve;

    public MoonBaseCurveModifier(NamespacedKey<DawnMoonInfo> moonKey, AnimationCurve curve)
    {
        _moonKey = moonKey;
        _curve = curve;
    }

    public NamespacedKey Key => DawnKeys.MoonBaseCurve;

    public WeightModifierPhase Phase => WeightModifierPhase.Base;

    public int Priority => -1000;

    public bool CanApply(WeightContext context)
    {
        return context.Moon?.TypedKey == _moonKey;
    }

    public void Apply(ref AnimationCurve? value, WeightContext context)
    {
        value = _curve;
    }
}