using UnityEngine;

namespace Dawn;

public sealed class MoonCurveModifier : IWeightModifier<AnimationCurve?>
{
    private readonly NamespacedKey<DawnMoonInfo> _moonOrTagKey;
    private readonly AnimationCurve _curve;

    public MoonCurveModifier(NamespacedKey<DawnMoonInfo> moonOrTagKey, AnimationCurve curve)
    {
        _moonOrTagKey = moonOrTagKey;
        _curve = curve;
    }

    public NamespacedKey Key => DawnKeys.MoonCurve;

    public WeightModifierPhase Phase => WeightModifierPhase.Override;

    public int Priority => 0;

    public bool CanApply(WeightContext context)
    {
        if (context.Moon == null)
            return false;

        if (context.Moon.TypedKey == _moonOrTagKey)
            return true;

        return context.Moon.HasTag(_moonOrTagKey);
    }

    public void Apply(ref AnimationCurve? value, WeightContext context)
    {
        value = _curve;
    }
}