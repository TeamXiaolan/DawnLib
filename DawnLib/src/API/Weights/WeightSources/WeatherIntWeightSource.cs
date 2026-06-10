using System.Collections.Generic;

namespace Dawn;

public sealed class WeatherIntWeightSource : IWeightModifierSource<int>
{
    private readonly List<UnresolvedNamespacedWeight> _weights;

    public WeatherIntWeightSource(List<UnresolvedNamespacedWeight> weights)
    {
        _weights = weights;
    }

    public void Build(WeightBuildContext context, List<IWeightModifier<int>> modifiers)
    {
        using NamespacedKeyResolver<DawnWeatherEffectInfo> resolver = new(context.Weathers.Values);
        foreach (UnresolvedNamespacedWeight unresolved in _weights)
        {
            ResolvedNamespacedWeight<DawnWeatherEffectInfo>? resolved = resolver.ResolveWeight(unresolved);

            if (resolved == null)
                continue;

            modifiers.Add(new WeatherIntWeightModifier(resolved.Value));
        }
    }
}