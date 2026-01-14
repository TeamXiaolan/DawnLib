using System;
using System.Collections.Generic;
using Dawn;
using Dawn.Internal;

namespace Dusk.Weights.Transformers;

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
            MatchingInteriorsWithWeightAndOperationDict[configWeight.NamespacedKey] = (configWeight.MathOperation, configWeight.Weight);
        }
    }

    public Dictionary<NamespacedKey, (MathOperation operation, float weight)> MatchingInteriorsWithWeightAndOperationDict = new();

    public override float GetNewWeight(float currentWeight, DawnDungeonInfo dungeonInfo)
    {
        return WeightTransformerTagLogic.ApplyByKeyOrTags(
            currentWeight,
            dungeonInfo.TypedKey,
            dungeonInfo.AllTags(),
            MatchingInteriorsWithWeightAndOperationDict,
            DoOperation,
            Debuggers.Weights
        );
    }

    public override MathOperation GetOperation(DawnDungeonInfo dungeonInfo)
    {
        if (MatchingInteriorsWithWeightAndOperationDict.TryGetValue(dungeonInfo.TypedKey, out var opWithWeight))
        {
            return opWithWeight.operation;
        }

        return MathOperation.Additive;
    }
}