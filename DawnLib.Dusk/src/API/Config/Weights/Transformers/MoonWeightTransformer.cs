using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Dawn.Internal;

namespace Dusk.Weights.Transformers;

[Serializable]
public class MoonWeightTransformer : WeightTransformer
{
    public MoonWeightTransformer(List<NamespacedConfigWeight> moonConfig)
    {
        if (moonConfig.Count <= 0)
            return;

        _moonConfig = moonConfig;
        foreach (NamespacedConfigWeight configWeight in moonConfig)
        {
            MatchingMoonsWithWeightAndOperationDict[configWeight.NamespacedKey] = (configWeight.MathOperation, configWeight.Weight);
        }

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
            Debuggers.Weights?.Log($"NamespacedKey: {namespacedKey}");
            operationWithWeight = MatchingMoonsWithWeightAndOperationDict[namespacedKey];
            currentWeight = DoOperation(currentWeight, operationWithWeight);
        }

        if (orderedAndValidTagNamespacedKeys.Count == 0)
        {
            return currentWeight;
        }

        currentWeight /= orderedAndValidTagNamespacedKeys.Count;
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