using System;
using System.Collections.Generic;
using Dawn;
using Dawn.Internal;

namespace Dusk.Weights;

[Serializable]
public class WeatherWeightTransformer : WeightTransformer<DawnWeatherEffectInfo>
{
    public WeatherWeightTransformer(List<UnresolvedNamespacedWeight> weatherConfig)
    {
        if (weatherConfig.Count <= 0)
            return;

        _weatherConfig = weatherConfig;
        LethalContent.Weathers.AfterTaggingWithContext += RegisterWeatherConfig;
    }

    private List<UnresolvedNamespacedWeight> _weatherConfig = new();

    private void RegisterWeatherConfig(NamespacedKeyResolver<DawnWeatherEffectInfo> weatherResolver)
    {
        MatchingWeathersWithWeightAndOperationDict.Clear();
        foreach (UnresolvedNamespacedWeight configWeight in _weatherConfig)
        {
            ResolvedNamespacedWeight<DawnWeatherEffectInfo>? resolvedWeight = weatherResolver.ResolveWeight(configWeight);
            if (resolvedWeight == null)
            {
                continue;
            }

            MatchingWeathersWithWeightAndOperationDict[resolvedWeight.Value.Key] = resolvedWeight.Value;
        }
    }

    public Dictionary<NamespacedKey, ResolvedNamespacedWeight<DawnWeatherEffectInfo>> MatchingWeathersWithWeightAndOperationDict = new();

    public override float GetNewWeight(float currentWeight, DawnWeatherEffectInfo weatherInfo)
    {
        if (!WeightTransformerTagLogic.TryApplyByKey(currentWeight, weatherInfo.TypedKey, MatchingWeathersWithWeightAndOperationDict, DoOperation, out float result, Debuggers.Weights))
        {
            result = WeightTransformerTagLogic.ApplyByTags(currentWeight, weatherInfo.AllTags(), MatchingWeathersWithWeightAndOperationDict, DoOperation, Debuggers.Weights);
        }

        return result;
    }

    public override MathOperation GetOperation(DawnWeatherEffectInfo weatherInfo)
    {
        NamespacedKey<DawnWeatherEffectInfo> typedKey = weatherInfo.TypedKey;
        if (MatchingWeathersWithWeightAndOperationDict.TryGetValue(typedKey, out ResolvedNamespacedWeight<DawnWeatherEffectInfo> opWithWeight))
        {
            return opWithWeight.Operation;
        }

        return MathOperation.Additive;
    }
}