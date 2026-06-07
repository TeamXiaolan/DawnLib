using System;
using System.Collections.Generic;
using Dawn;
using Dawn.Internal;

namespace Dusk.Weights;

[Serializable]
public class WeatherWeightTransformer : WeightTransformer<DawnWeatherEffectInfo>
{
    public WeatherWeightTransformer(List<NamespacedConfigWeight> weatherConfig)
    {
        if (weatherConfig.Count <= 0)
            return;

        _weatherConfig = weatherConfig;
        RegisterWeatherConfig();
    }

    private List<NamespacedConfigWeight> _weatherConfig = new();

    private void RegisterWeatherConfig()
    {
        MatchingWeathersWithWeightAndOperationDict.Clear();
        foreach (NamespacedConfigWeight configWeight in _weatherConfig)
        {
            MatchingWeathersWithWeightAndOperationDict[configWeight.NamespacedKey] = configWeight;
        }
    }

    public Dictionary<NamespacedKey, NamespacedConfigWeight> MatchingWeathersWithWeightAndOperationDict = new();

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
        if (MatchingWeathersWithWeightAndOperationDict.TryGetValue(typedKey, out NamespacedConfigWeight opWithWeight))
        {
            return opWithWeight.Operation;
        }

        return MathOperation.Additive;
    }
}