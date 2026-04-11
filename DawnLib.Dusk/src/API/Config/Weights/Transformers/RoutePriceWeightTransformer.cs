using System;
using System.Collections.Generic;
using System.Linq;

namespace Dusk.Weights;

[Serializable]
public class RoutePriceWeightTransformer : WeightTransformer<int>
{
    private readonly List<IntComparisonConfigWeight> _routePriceConfig = new();

    public RoutePriceWeightTransformer(List<IntComparisonConfigWeight> routePriceConfig)
    {
        if (routePriceConfig.Count <= 0)
            return;

        _routePriceConfig = routePriceConfig;
    }

    public override float GetNewWeight(float currentWeight, int routePrice)
    {
        List<IntComparisonConfigWeight> matches = GetOrderedMatches(routePrice);

        foreach (IntComparisonConfigWeight match in matches)
        {
            currentWeight = DoOperation(currentWeight, match);
        }

        return currentWeight;
    }

    public override MathOperation GetOperation(int routePrice)
    {
        List<IntComparisonConfigWeight> matches = GetOrderedMatches(routePrice);

        if (matches.Count == 0)
        {
            return MathOperation.Additive;
        }

        return matches[0].Operation;
    }

    private List<IntComparisonConfigWeight> GetOrderedMatches(int routePrice)
    {
        List<IntComparisonConfigWeight> matches = new();

        foreach (IntComparisonConfigWeight config in _routePriceConfig)
        {
            if (Matches(routePrice, config.IntComparison))
            {
                matches.Add(config);
            }
        }

        return matches
            .OrderByDescending(x =>
                x.Operation == MathOperation.Additive ||
                x.Operation == MathOperation.Subtractive)
            .ToList();
    }

    private static bool Matches(int routePrice, IntComparison comparison)
    {
        return comparison.ComparisonOperation switch
        {
            ComparisonOperation.Equal => routePrice == comparison.Value,
            ComparisonOperation.NotEqual => routePrice != comparison.Value,
            ComparisonOperation.Greater => routePrice > comparison.Value,
            ComparisonOperation.Less => routePrice < comparison.Value,
            ComparisonOperation.GreaterOrEqual => routePrice >= comparison.Value,
            ComparisonOperation.LessOrEqual => routePrice <= comparison.Value,
            _ => false
        };
    }
}