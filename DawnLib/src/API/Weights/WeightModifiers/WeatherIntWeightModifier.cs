namespace Dawn;

public sealed class WeatherIntWeightModifier : IWeightModifier<int>
{
    private readonly ResolvedNamespacedWeight<DawnWeatherEffectInfo> _weight;

    public WeatherIntWeightModifier(ResolvedNamespacedWeight<DawnWeatherEffectInfo> weight)
    {
        _weight = weight;
    }

    public NamespacedKey Key => DawnKeys.WeatherIntWeight;

    public WeightModifierPhase Phase => IntWeightOperations.GetPhase(_weight.Operation);

    public int Priority => 0;

    public bool CanApply(WeightContext context)
    {
        if (context.Weather == null)
            return false;

        if (context.Weather.TypedKey == _weight.Key)
            return true;

        return context.Weather.HasTag(_weight.Key);
    }

    public void Apply(ref int value, WeightContext context)
    {
        IntWeightOperations.Apply(ref value, _weight.Operation, _weight.Value);
    }
}