using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;

namespace Dusk.Weights.Transformers;

[Serializable]
public class MoonWeightTransformer : WeightTransformer
{
    public MoonWeightTransformer(List<NamespacedConfigWeight> moonConfig)
    {
        if (moonConfig.Count <= 0)
            return;

        foreach (NamespacedConfigWeight configWeight in moonConfig)
        {
            MatchingMoonsWithWeightAndOperationDict[configWeight.NamespacedKey] = (configWeight.MathOperation, configWeight.Weight);
        }
    }

    public Dictionary<NamespacedKey, (MathOperation operation, float weight)> MatchingMoonsWithWeightAndOperationDict = new();

    public override float GetNewWeight(float currentWeight)
    {
        if (!RoundManager.Instance) return currentWeight;
        if (!RoundManager.Instance.currentLevel) return currentWeight;
        DawnMoonInfo moonInfo = RoundManager.Instance.currentLevel.GetDawnInfo();
        if (MatchingMoonsWithWeightAndOperationDict.TryGetValue(moonInfo.TypedKey, out (MathOperation operation, float weight) operationWithWeight))
        {
            return DoOperation(currentWeight, operationWithWeight);
        }

        List<NamespacedKey> orderedAndValidTagNamespacedKeys = new();
        HashSet<string> processedKeys = new();

        foreach (NamespacedKey tagNamespacedKey in moonInfo.AllTags())
        {
            if (!processedKeys.Add(tagNamespacedKey.Key))
                continue;

            foreach (NamespacedKey moonNamespacedKey in MatchingMoonsWithWeightAndOperationDict.Keys)
            {
                if (moonNamespacedKey.Key == tagNamespacedKey.Key)
                {
                    orderedAndValidTagNamespacedKeys.Add(moonNamespacedKey);
                    break;
                }
            }
        }

        orderedAndValidTagNamespacedKeys = orderedAndValidTagNamespacedKeys.OrderBy(x => MatchingMoonsWithWeightAndOperationDict[x].operation == MathOperation.Additive || MatchingMoonsWithWeightAndOperationDict[x].operation == MathOperation.Subtractive).ToList();
        foreach (NamespacedKey namespacedKey in orderedAndValidTagNamespacedKeys)
        {
            operationWithWeight = MatchingMoonsWithWeightAndOperationDict[namespacedKey];
            currentWeight = DoOperation(currentWeight, operationWithWeight);
        }

        return currentWeight;
    }

    public override MathOperation GetOperation()
    {
        if (!RoundManager.Instance) return MathOperation.Additive;
        if (!RoundManager.Instance.currentLevel) return MathOperation.Additive;
        DawnMoonInfo moonInfo = RoundManager.Instance.currentLevel.GetDawnInfo();
        if (!MatchingMoonsWithWeightAndOperationDict.TryGetValue(moonInfo.TypedKey, out (MathOperation operation, float weight) operationWithWeight)) return MathOperation.Additive;

        return operationWithWeight.operation;
    }
}