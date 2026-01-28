using System;
using System.Collections.Generic;
using Dawn;
using Dawn.Internal;

namespace Dusk.Weights.Transformers;

[Serializable]
public class MoonWeightTransformer : WeightTransformer<DawnMoonInfo>
{
    public MoonWeightTransformer(List<NamespacedConfigWeight> moonConfig)
    {
        if (moonConfig.Count <= 0)
            return;

        _moonConfig = moonConfig;
        ReregisterMoonConfig();
        LethalContent.Moons.OnFreeze += ReregisterMoonConfig;
    }

    private List<NamespacedConfigWeight> _moonConfig = new();

    private void ReregisterMoonConfig()
    {
        MatchingMoonsWithWeightAndOperationDict.Clear();
        foreach (NamespacedConfigWeight configWeight in _moonConfig)
        {
            MatchingMoonsWithWeightAndOperationDict[configWeight.NamespacedKey] = (configWeight.MathOperation, configWeight.Weight);
        }
    }

    public Dictionary<NamespacedKey, (MathOperation operation, float weight)> MatchingMoonsWithWeightAndOperationDict = new();

    public override float GetNewWeight(float currentWeight, DawnMoonInfo moonInfo)
    {
        return WeightTransformerTagLogic.ApplyByKeyOrTags(
            currentWeight,
            moonInfo.TypedKey,
            moonInfo.AllTags(),
            MatchingMoonsWithWeightAndOperationDict,
            DoOperation,
            Debuggers.Weights
        );
    }

    public override MathOperation GetOperation(DawnMoonInfo moonInfo)
    {
        if (MatchingMoonsWithWeightAndOperationDict.TryGetValue(moonInfo.TypedKey, out var opWithWeight))
        {
            return opWithWeight.operation;
        }

        return MathOperation.Additive;
    }
}
