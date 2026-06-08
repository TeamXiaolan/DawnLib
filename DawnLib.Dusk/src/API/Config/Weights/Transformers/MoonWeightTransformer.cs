using System;
using System.Collections.Generic;
using Dawn;
using Dawn.Internal;

namespace Dusk.Weights;

[Serializable]
public class MoonWeightTransformer : WeightTransformer<DawnMoonInfo>
{
    public MoonWeightTransformer(List<UnresolvedNamespacedWeight> moonConfig)
    {
        if (moonConfig.Count <= 0)
            return;

        _moonConfig = moonConfig;
        LethalContent.Moons.AfterTaggingWithContext += RegisterMoonConfig;
    }

    private List<UnresolvedNamespacedWeight> _moonConfig = new();

    private void RegisterMoonConfig(NamespacedKeyResolver<DawnMoonInfo> moonResolver)
    {
        MatchingMoonsWithWeightAndOperationDict.Clear();
        foreach (UnresolvedNamespacedWeight configWeight in _moonConfig)
        {
            ResolvedNamespacedWeight<DawnMoonInfo>? resolvedWeight = moonResolver.ResolveWeight(configWeight);
            if (resolvedWeight == null)
            {
                continue;
            }

            MatchingMoonsWithWeightAndOperationDict[resolvedWeight.Value.Key] = resolvedWeight.Value;
        }
    }

    public Dictionary<NamespacedKey, ResolvedNamespacedWeight<DawnMoonInfo>> MatchingMoonsWithWeightAndOperationDict = new();

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
        if (MatchingMoonsWithWeightAndOperationDict.TryGetValue(moonInfo.TypedKey, out ResolvedNamespacedWeight<DawnMoonInfo> opWithWeight))
        {
            return opWithWeight.Operation;
        }

        return MathOperation.Additive;
    }
}
