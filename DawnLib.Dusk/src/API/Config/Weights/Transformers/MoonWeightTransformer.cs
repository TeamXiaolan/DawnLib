using System;
using System.Collections.Generic;
using Dawn;
using Dawn.Internal;

namespace Dusk.Weights;

[Serializable]
public class MoonWeightTransformer : WeightTransformer<DawnMoonInfo>
{
    public MoonWeightTransformer(List<NamespacedConfigWeight> moonConfig)
    {
        if (moonConfig.Count <= 0)
            return;

        _moonConfig = moonConfig;
        RegisterMoonConfig();
    }

    private List<NamespacedConfigWeight> _moonConfig = new();

    private void RegisterMoonConfig()
    {
        MatchingMoonsWithWeightAndOperationDict.Clear();
        foreach (NamespacedConfigWeight configWeight in _moonConfig)
        {
            MatchingMoonsWithWeightAndOperationDict[configWeight.NamespacedKey] = configWeight;
        }
    }

    public Dictionary<NamespacedKey, NamespacedConfigWeight> MatchingMoonsWithWeightAndOperationDict = new();

    public override float GetNewWeight(float currentWeight, DawnMoonInfo moonInfo)
    {
        if (!WeightTransformerTagLogic.TryApplyByKey(currentWeight, moonInfo.TypedKey, MatchingMoonsWithWeightAndOperationDict, DoOperation, out float result, Debuggers.Weights))
        {
            result = WeightTransformerTagLogic.ApplyByTags(currentWeight, moonInfo.AllTags(), MatchingMoonsWithWeightAndOperationDict, DoOperation, Debuggers.Weights);
        }

        return result;
    }

    public override MathOperation GetOperation(DawnMoonInfo moonInfo)
    {
        if (MatchingMoonsWithWeightAndOperationDict.TryGetValue(moonInfo.TypedKey, out NamespacedConfigWeight opWithWeight))
        {
            return opWithWeight.Operation;
        }

        return MathOperation.Additive;
    }
}
