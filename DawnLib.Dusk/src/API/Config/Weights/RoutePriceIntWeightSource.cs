using System;
using System.Collections.Generic;
using Dawn;

namespace Dusk.Weights;

public sealed class RoutePriceIntWeightSource : WeightModifierSource<int>
{
    private readonly Func<IEnumerable<IntComparisonConfigWeight>> _getConfigs;

    public RoutePriceIntWeightSource(Func<IEnumerable<IntComparisonConfigWeight>> getConfigs)
    {
        _getConfigs = getConfigs;
    }

    public override void Build(WeightBuildContext context, List<IWeightModifier<int>> modifiers)
    {
        foreach (IntComparisonConfigWeight config in _getConfigs())
        {
            modifiers.Add(new RoutePriceIntWeightModifier(config));
        }
    }
}