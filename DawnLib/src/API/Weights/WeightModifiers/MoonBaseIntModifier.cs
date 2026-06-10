namespace Dawn;

public sealed class MoonBaseIntModifier : IWeightModifier<int>
{
    private readonly NamespacedKey<DawnMoonInfo> _moonKey;
    private readonly int _value;

    public MoonBaseIntModifier(NamespacedKey<DawnMoonInfo> moonKey, int value)
    {
        _moonKey = moonKey;
        _value = value;
    }

    public NamespacedKey Key => DawnKeys.MoonBaseInt;

    public WeightModifierPhase Phase => WeightModifierPhase.Base;

    public int Priority => -1000;

    public bool CanApply(WeightContext context)
    {
        return context.Moon?.TypedKey == _moonKey;
    }

    public void Apply(ref int value, WeightContext context)
    {
        value = _value;
    }
}