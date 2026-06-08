using System;
using System.Collections.Generic;
using Dawn;
using Dawn.Internal;

namespace Dusk.Weights;

[Serializable]
public class InteriorWeightTransformer : WeightTransformer<DawnDungeonInfo>
{
    public InteriorWeightTransformer(List<UnresolvedNamespacedWeight> interiorConfig)
    {
        if (interiorConfig.Count <= 0)
            return;

        _dungeonConfig = interiorConfig;
        LethalContent.Dungeons.AfterTaggingWithContext += RegisterDungeonConfig;
    }

    private List<UnresolvedNamespacedWeight> _dungeonConfig = new();

    private void RegisterDungeonConfig(NamespacedKeyResolver<DawnDungeonInfo> dungeonResolver)
    {
        MatchingInteriorsWithWeightAndOperationDict.Clear();
        foreach (UnresolvedNamespacedWeight configWeight in _dungeonConfig)
        {
            ResolvedNamespacedWeight<DawnDungeonInfo>? resolvedWeight = dungeonResolver.ResolveWeight(configWeight);
            if (resolvedWeight == null)
            {
                continue;
            }

            MatchingInteriorsWithWeightAndOperationDict[resolvedWeight.Value.Key] = resolvedWeight.Value;
        }
    }

    public Dictionary<NamespacedKey, ResolvedNamespacedWeight<DawnDungeonInfo>> MatchingInteriorsWithWeightAndOperationDict = new();

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
        if (MatchingInteriorsWithWeightAndOperationDict.TryGetValue(dungeonInfo.TypedKey, out ResolvedNamespacedWeight<DawnDungeonInfo> opWithWeight))
        {
            return opWithWeight.Operation;
        }

        return MathOperation.Additive;
    }
}