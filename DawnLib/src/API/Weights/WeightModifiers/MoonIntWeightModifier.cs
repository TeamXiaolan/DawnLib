namespace Dawn;

public sealed class MoonIntWeightModifier : IWeightModifier<int>
{
    private readonly ResolvedNamespacedWeight<DawnMoonInfo> _weight;

    public MoonIntWeightModifier(ResolvedNamespacedWeight<DawnMoonInfo> weight)
    {
        _weight = weight;
    }

    public NamespacedKey Key => DawnKeys.MoonIntWeight;

    public WeightModifierPhase Phase => IntWeightOperations.GetPhase(_weight.Operation);

    public int Priority => 0;

    public bool CanApply(WeightContext context)
    {
        if (context.Moon == null)
            return false;

        if (context.Moon.TypedKey == _weight.Key)
            return true;

        foreach (NamespacedKey tag in context.Moon.AllTags())
        {
            if (tag.Key == _weight.Key.Key)
            {
                return true;
            }
        }

        return false;
    }

    public void Apply(ref int value, WeightContext context)
    {
        IntWeightOperations.Apply(ref value, _weight.Operation, _weight.Value);
    }
}