namespace Dawn;

public sealed class WeatherIntWeightModifier : IWeightModifier<int>
{
    private readonly ResolvedNamespacedWeight _weight;

    public WeatherIntWeightModifier(ResolvedNamespacedWeight weight)
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

        if (_weight.Key is NamespacedKey<DawnWeatherEffectInfo> typedKey)
        {
            return typedKey == context.Weather.TypedKey;
        }

        foreach (NamespacedKey tag in context.Weather.AllTags())
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