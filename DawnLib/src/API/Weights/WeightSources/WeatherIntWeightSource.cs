using System;
using System.Collections.Generic;
using Dawn.Internal;

namespace Dawn;

public sealed class WeatherIntWeightSource : WeightModifierSource<int>
{
    private readonly Func<IEnumerable<UnresolvedNamespacedWeight>> _getWeights;

    public WeatherIntWeightSource(Func<IEnumerable<UnresolvedNamespacedWeight>> getWeights)
    {
        _getWeights = getWeights;
    }

    public override void Build(WeightBuildContext context, List<IWeightModifier<int>> modifiers)
    {
        using NamespacedKeyResolver<DawnWeatherEffectInfo> resolver = new(context.Weathers.Values);
        foreach (UnresolvedNamespacedWeight unresolved in _getWeights())
        {
            Debuggers.Weights?.Log($"Building WeatherIntWeightSource with input {unresolved.KeyInput} and value {unresolved.Value}");
            ResolvedNamespacedWeight<DawnWeatherEffectInfo>? resolved = resolver.ResolveWeight(unresolved);

            if (resolved == null)
                continue;

            modifiers.Add(new WeatherIntWeightModifier(resolved.Value));
        }
    }
}