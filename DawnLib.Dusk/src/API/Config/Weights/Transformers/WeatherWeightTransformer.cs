using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;

namespace Dusk.Weights.Transformers;

[Serializable]
public class WeatherWeightTransformer : WeightTransformer
{
    public WeatherWeightTransformer(List<NamespacedConfigWeight> weatherConfig)
    {
        if (weatherConfig.Count <= 0)
            return;

        foreach (NamespacedConfigWeight configWeight in weatherConfig)
        {
            MatchingWeathersWithWeightAndOperationDict[configWeight.NamespacedKey] = (configWeight.MathOperation, configWeight.Weight);
        }
    }
    public Dictionary<NamespacedKey, (MathOperation operation, float weight)> MatchingWeathersWithWeightAndOperationDict = new();

    public override float GetNewWeight(float currentWeight)
    {
        if (!TimeOfDay.Instance) return currentWeight;
        if (!TimeOfDay.Instance.currentLevel) return currentWeight;

        NamespacedKey currentWeatherNamespacedKey = NamespacedKey<DawnWeatherEffectInfo>.Vanilla("none");
        IEnumerable<NamespacedKey> allTags = [];
        if (TimeOfDay.Instance.currentLevel.currentWeather != LevelWeatherType.None)
        {
            DawnWeatherEffectInfo? weatherInfo = TimeOfDay.Instance.effects[(int)TimeOfDay.Instance.currentLevel.currentWeather].GetDawnInfo();
            if (weatherInfo == null)
            {
                DawnPlugin.Logger.LogError($"Could not find weather info for {TimeOfDay.Instance.currentLevel.currentWeather},");
                return currentWeight;
            }
            currentWeatherNamespacedKey = weatherInfo.TypedKey;
            allTags = weatherInfo.AllTags();
        }

        if (MatchingWeathersWithWeightAndOperationDict.TryGetValue(currentWeatherNamespacedKey, out (MathOperation operation, float weight) operationWithWeight))
        {
            return DoOperation(currentWeight, operationWithWeight);
        }

        List<NamespacedKey> orderedAndValidTagNamespacedKeys = new();
        foreach (NamespacedKey tagNamespacedKey in allTags)
        {
            foreach (NamespacedKey weatherNamespacedKey in MatchingWeathersWithWeightAndOperationDict.Keys)
            {
                if (weatherNamespacedKey.Key == tagNamespacedKey.Key)
                {
                    orderedAndValidTagNamespacedKeys.Add(tagNamespacedKey);
                    break;
                }
            }
        }

        orderedAndValidTagNamespacedKeys = orderedAndValidTagNamespacedKeys.OrderBy(x => MatchingWeathersWithWeightAndOperationDict[x].operation == MathOperation.Additive || MatchingWeathersWithWeightAndOperationDict[x].operation == MathOperation.Subtractive).ToList();
        foreach (NamespacedKey namespacedKey in orderedAndValidTagNamespacedKeys)
        {
            operationWithWeight = MatchingWeathersWithWeightAndOperationDict[namespacedKey];
            currentWeight = DoOperation(currentWeight, operationWithWeight);
        }

        return currentWeight;
    }

    public override MathOperation GetOperation()
    {
        if (!TimeOfDay.Instance) return MathOperation.Additive;
        if (!TimeOfDay.Instance.currentLevel) return MathOperation.Additive;

        NamespacedKey currentWeatherNamespacedKey = NamespacedKey<DawnWeatherEffectInfo>.Vanilla("none");
        if (TimeOfDay.Instance.currentLevel.currentWeather != LevelWeatherType.None)
        {
            DawnWeatherEffectInfo? weatherInfo = TimeOfDay.Instance.effects[(int)TimeOfDay.Instance.currentLevel.currentWeather].GetDawnInfo();
            if (weatherInfo == null)
            {
                DawnPlugin.Logger.LogError($"Could not find weather info for {TimeOfDay.Instance.currentLevel.currentWeather},");
                return MathOperation.Additive;
            }
            currentWeatherNamespacedKey = weatherInfo.TypedKey;
        }

        if (!MatchingWeathersWithWeightAndOperationDict.TryGetValue(currentWeatherNamespacedKey, out (MathOperation operation, float weight) operationWithWeight)) return MathOperation.Additive;

        return operationWithWeight.operation;
    }
}