namespace Dawn;

public sealed class GlobalBaseIntModifier : IWeightModifier<int>
{
    private readonly int _value;

    public GlobalBaseIntModifier(int value)
    {
        _value = value;
    }

    public NamespacedKey Key => DawnKeys.GlobalBaseInt;

    public WeightModifierPhase Phase => WeightModifierPhase.Base;

    public int Priority => -1000;

    public bool CanApply(WeightContext context)
    {
        return true;
    }

    public void Apply(ref int value, WeightContext context)
    {
        value = _value;
    }
}