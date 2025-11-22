using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Dawn.Internal;

namespace Dusk.Weights.Transformers;
[Serializable]
public class InteriorWeightTransformer : WeightTransformer
{
    public InteriorWeightTransformer(List<NamespacedConfigWeight> interiorConfig)
    {
        if (interiorConfig.Count <= 0)
            return;

        foreach (NamespacedConfigWeight configWeight in interiorConfig)
        {
            MatchingInteriorsWithWeightAndOperationDict[configWeight.NamespacedKey] = (configWeight.MathOperation, configWeight.Weight);
        }
    }

    public Dictionary<NamespacedKey, (MathOperation operation, float weight)> MatchingInteriorsWithWeightAndOperationDict = new();

    public override float GetNewWeight(float currentWeight)
    {
        if (!RoundManager.Instance) return currentWeight;
        if (!RoundManager.Instance.dungeonGenerator) return currentWeight;
        if (!RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow) return currentWeight;
        DawnDungeonInfo dungeonInfo = RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.GetDawnInfo();
        if (MatchingInteriorsWithWeightAndOperationDict.TryGetValue(dungeonInfo.TypedKey, out (MathOperation operation, float weight) operationWithWeight))
        {
            return DoOperation(currentWeight, operationWithWeight);
        }

        List<NamespacedKey> orderedAndValidTagNamespacedKeys = new();
        foreach (NamespacedKey tagNamespacedKey in dungeonInfo.AllTags())
        {
            foreach (NamespacedKey interiorNamespacedKey in MatchingInteriorsWithWeightAndOperationDict.Keys)
            {
                if (interiorNamespacedKey.Key == tagNamespacedKey.Key)
                {
                    orderedAndValidTagNamespacedKeys.Add(tagNamespacedKey);
                    break;
                }
            }
        }

        orderedAndValidTagNamespacedKeys = orderedAndValidTagNamespacedKeys.OrderBy(x => MatchingInteriorsWithWeightAndOperationDict[x].operation == MathOperation.Additive || MatchingInteriorsWithWeightAndOperationDict[x].operation == MathOperation.Subtractive).ToList();
        foreach (NamespacedKey namespacedKey in orderedAndValidTagNamespacedKeys)
        {
            operationWithWeight = MatchingInteriorsWithWeightAndOperationDict[namespacedKey];
            currentWeight = DoOperation(currentWeight, operationWithWeight);
        }

        return currentWeight;
    }

    public override MathOperation GetOperation()
    {
        if (!RoundManager.Instance) return MathOperation.Additive;
        if (!RoundManager.Instance.dungeonGenerator) return MathOperation.Additive;
        if (!RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow) return MathOperation.Additive;
        DawnDungeonInfo dungeonInfo = RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.GetDawnInfo();
        if (!MatchingInteriorsWithWeightAndOperationDict.TryGetValue(dungeonInfo.TypedKey, out (MathOperation operation, float weight) operationWithWeight)) return MathOperation.Additive;

        return operationWithWeight.operation;
    }
}