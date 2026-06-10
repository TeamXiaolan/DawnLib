using System.Collections.Generic;
using Dawn;

namespace Dusk.Weights;

public sealed class RoutePriceIntWeightSource : IWeightModifierSource<int>
{
    private readonly List<IntComparisonConfigWeight> _configs;

    public RoutePriceIntWeightSource(List<IntComparisonConfigWeight> configs)
    {
        _configs = configs;
    }

    public void Build(WeightBuildContext context, List<IWeightModifier<int>> modifiers)
    {
        foreach (IntComparisonConfigWeight config in _configs)
        {
            modifiers.Add(new RoutePriceIntWeightModifier(config));
        }
    }
}