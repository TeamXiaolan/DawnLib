using System;
using System.Collections.Generic;
using Dawn;
using Dawn.Internal;

namespace Dusk.Weights;

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
            MatchingWeathersWithWeightAndOperationDict[configWeight.NamespacedKey] = configWeight;
        }
    }

    public Dictionary<NamespacedKey, NamespacedConfigWeight> MatchingWeathersWithWeightAndOperationDict = new();

    public override float GetNewWeight(float currentWeight, DawnWeatherEffectInfo? weatherInfo)
    {
        NamespacedKey<DawnWeatherEffectInfo> typedKey = weatherInfo?.TypedKey ?? WeatherKeys.None;
        IEnumerable<NamespacedKey> tags = weatherInfo?.AllTags() ?? Array.Empty<NamespacedKey>();

        if (!WeightTransformerTagLogic.TryApplyByKey(currentWeight, typedKey, MatchingWeathersWithWeightAndOperationDict, DoOperation, out float result, Debuggers.Weights))
        {
            result = WeightTransformerTagLogic.ApplyByTags(currentWeight, tags, MatchingWeathersWithWeightAndOperationDict, DoOperation, Debuggers.Weights);
        }

        return result;
    }

    public override MathOperation GetOperation(DawnWeatherEffectInfo? weatherInfo)
    {
        NamespacedKey<DawnWeatherEffectInfo> typedKey = weatherInfo?.TypedKey ?? NamespacedKey<DawnWeatherEffectInfo>.Vanilla("none");
        if (MatchingWeathersWithWeightAndOperationDict.TryGetValue(typedKey, out NamespacedConfigWeight opWithWeight))
        {
            return opWithWeight.Operation;
        }

        return MathOperation.Additive;
    }
}