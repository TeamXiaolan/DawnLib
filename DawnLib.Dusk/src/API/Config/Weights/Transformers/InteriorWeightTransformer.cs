using System;
using System.Collections.Generic;
using Dawn;
using Dawn.Internal;

namespace Dusk.Weights;

[Serializable]
public class InteriorWeightTransformer : WeightTransformer<DawnDungeonInfo>
{
    public InteriorWeightTransformer(List<NamespacedConfigWeight> interiorConfig)
    {
        if (interiorConfig.Count <= 0)
            return;

        _dungeonConfig = interiorConfig;
        ReregisterDungeonConfig();
        LethalContent.Dungeons.OnFreeze += ReregisterDungeonConfig;
    }

    private List<NamespacedConfigWeight> _dungeonConfig = new();

    private void ReregisterDungeonConfig()
    {
        MatchingInteriorsWithWeightAndOperationDict.Clear();
        foreach (NamespacedConfigWeight configWeight in _dungeonConfig)
        {
            MatchingInteriorsWithWeightAndOperationDict[configWeight.NamespacedKey] = configWeight;
        }
    }

    public Dictionary<NamespacedKey, NamespacedConfigWeight> MatchingInteriorsWithWeightAndOperationDict = new();

    public override float GetNewWeight(float currentWeight, DawnDungeonInfo dungeonInfo)
    {
        if (!WeightTransformerTagLogic.TryApplyByKey(currentWeight, dungeonInfo.TypedKey, MatchingInteriorsWithWeightAndOperationDict, DoOperation, out float result, Debuggers.Weights))
        {
            result = WeightTransformerTagLogic.ApplyByTags(currentWeight, dungeonInfo.AllTags(), MatchingInteriorsWithWeightAndOperationDict, DoOperation, Debuggers.Weights);
        }

        return result;
    }

    public override MathOperation GetOperation(DawnDungeonInfo dungeonInfo)
    {
        if (MatchingInteriorsWithWeightAndOperationDict.TryGetValue(dungeonInfo.TypedKey, out NamespacedConfigWeight opWithWeight))
        {
            return opWithWeight.Operation;
        }

        return MathOperation.Additive;
    }
}