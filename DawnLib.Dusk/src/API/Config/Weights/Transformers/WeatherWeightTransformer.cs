using System;
using System.Collections.Generic;
using Dawn;
using Dawn.Internal;

namespace Dusk.Weights.Transformers;

[Serializable]
public class WeatherWeightTransformer : WeightTransformer<DawnWeatherEffectInfo?>
{
    public WeatherWeightTransformer(List<NamespacedConfigWeight> weatherConfig)
    {
        if (weatherConfig.Count <= 0)
            return;

        _weatherConfig = weatherConfig;
        ReregisterWeatherConfig();
        LethalContent.Weathers.OnFreeze += ReregisterWeatherConfig;
    }

    private List<NamespacedConfigWeight> _weatherConfig = new();

    private void ReregisterWeatherConfig()
    {
        MatchingWeathersWithWeightAndOperationDict.Clear();
        foreach (NamespacedConfigWeight configWeight in _weatherConfig)
        {
            MatchingWeathersWithWeightAndOperationDict[configWeight.NamespacedKey] = (configWeight.MathOperation, configWeight.Weight);
        }
    }

    public Dictionary<NamespacedKey, (MathOperation operation, float weight)> MatchingWeathersWithWeightAndOperationDict = new();

    public override float GetNewWeight(float currentWeight, DawnWeatherEffectInfo? weatherInfo)
    {
        NamespacedKey<DawnWeatherEffectInfo> typedKey = weatherInfo?.TypedKey ?? NamespacedKey<DawnWeatherEffectInfo>.Vanilla("none");
        IEnumerable<NamespacedKey> tags = weatherInfo?.AllTags() ?? Array.Empty<NamespacedKey>();

        return WeightTransformerTagLogic.ApplyByKeyOrTags(
            currentWeight,
            typedKey,
            tags,
            MatchingWeathersWithWeightAndOperationDict,
            DoOperation,
            Debuggers.Weights
        );
    }

    public override MathOperation GetOperation(DawnWeatherEffectInfo? weatherInfo)
    {
        NamespacedKey<DawnWeatherEffectInfo> typedKey = weatherInfo?.TypedKey ?? NamespacedKey<DawnWeatherEffectInfo>.Vanilla("none");
        if (MatchingWeathersWithWeightAndOperationDict.TryGetValue(typedKey, out var opWithWeight))
        {
            return opWithWeight.operation;
        }

        return MathOperation.Additive;
    }
}