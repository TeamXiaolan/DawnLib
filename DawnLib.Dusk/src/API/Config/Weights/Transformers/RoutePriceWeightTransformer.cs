using System;
using System.Collections.Generic;
using Dawn.Internal;

namespace Dusk.Weights;

[Serializable]
public class RoutePriceWeightTransformer : WeightTransformer<int>
{
    public RoutePriceWeightTransformer(List<IntComparisonConfigWeight> routePriceConfig)
    {
        if (routePriceConfig.Count <= 0)
            return;

        _routePriceConfig = routePriceConfig;
        foreach (IntComparisonConfigWeight configWeight in _routePriceConfig)
        {
            MatchingRoutePricesWithWeightAndOperationDict[configWeight.IntComparison.Value] = configWeight;
        }
    }

    private List<IntComparisonConfigWeight> _routePriceConfig = new();

    public Dictionary<int, IntComparisonConfigWeight> MatchingRoutePricesWithWeightAndOperationDict = new();

    public override float GetNewWeight(float currentWeight, int routePrice)
    {
        if (!WeightTransformerTagLogic.TryApplyByKey(currentWeight, routePrice, MatchingRoutePricesWithWeightAndOperationDict, DoOperation, out float result, Debuggers.Weights))
        {
            return 0;
        }

        return result;
    }

    public override MathOperation GetOperation(int routePrice)
    {
        if (MatchingRoutePricesWithWeightAndOperationDict.TryGetValue(routePrice, out IntComparisonConfigWeight opWithWeight))
        {
            return opWithWeight.Operation;
        }

        return MathOperation.Additive;
    }
}
