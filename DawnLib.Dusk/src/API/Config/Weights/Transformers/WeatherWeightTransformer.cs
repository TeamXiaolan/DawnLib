using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Dawn.Internal;

namespace Dusk.Weights.Transformers;

[Serializable]
public class WeatherWeightTransformer : WeightTransformer
{
    public WeatherWeightTransformer(List<NamespacedConfigWeight> weatherConfig)
    {
        if (weatherConfig.Count <= 0)
            return;

        _weatherConfig = weatherConfig;
        foreach (NamespacedConfigWeight configWeight in weatherConfig)
        {
            MatchingWeathersWithWeightAndOperationDict[configWeight.NamespacedKey] = (configWeight.MathOperation, configWeight.Weight);
        }

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
            Debuggers.Weights?.Log($"NamespacedKey: {currentWeatherNamespacedKey}");
            return DoOperation(currentWeight, operationWithWeight);
        }

        List<NamespacedKey> orderedAndValidTagNamespacedKeys = new();
        HashSet<string> processedKeys = new();

        foreach (NamespacedKey tagNamespacedKey in allTags)
        {
            if (!processedKeys.Add(tagNamespacedKey.Key))
                continue;

            foreach (NamespacedKey moonNamespacedKey in MatchingWeathersWithWeightAndOperationDict.Keys)
            {
                if (moonNamespacedKey.Key == tagNamespacedKey.Key)
                {
                    orderedAndValidTagNamespacedKeys.Add(moonNamespacedKey);
                    break;
                }
            }
        }

        orderedAndValidTagNamespacedKeys = orderedAndValidTagNamespacedKeys.OrderBy(x => MatchingWeathersWithWeightAndOperationDict[x].operation == MathOperation.Additive || MatchingWeathersWithWeightAndOperationDict[x].operation == MathOperation.Subtractive).ToList();
        foreach (NamespacedKey namespacedKey in orderedAndValidTagNamespacedKeys)
        {
            Debuggers.Weights?.Log($"NamespacedKey: {namespacedKey}");
            operationWithWeight = MatchingWeathersWithWeightAndOperationDict[namespacedKey];
            currentWeight = DoOperation(currentWeight, operationWithWeight);
        }

        if (orderedAndValidTagNamespacedKeys.Count == 0)
        {
            return currentWeight;
        }

        currentWeight /= orderedAndValidTagNamespacedKeys.Count;
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