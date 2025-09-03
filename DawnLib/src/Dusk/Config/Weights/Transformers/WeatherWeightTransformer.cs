using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Dawn.Dusk;

[Serializable]
public class WeatherWeightTransformer : WeightTransformer
{
    public WeatherWeightTransformer(string weatherConfig)
    {
        if (string.IsNullOrEmpty(weatherConfig))
            return;

        FromConfigString(weatherConfig);
    }
    public Dictionary<NamespacedKey, string> MatchingWeathersWithWeightAndOperationDict = new();

    public override string ToConfigString()
    {
        if (MatchingWeathersWithWeightAndOperationDict.Count == 0)
            return string.Empty;

        string MatchingWeatherWithWeight = string.Join(",", MatchingWeathersWithWeightAndOperationDict.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        return $"{MatchingWeatherWithWeight}";
    }

    public override void FromConfigString(string config)
    {
        if (string.IsNullOrEmpty(config))
            return;

        MatchingWeathersWithWeightAndOperationDict.Clear();
        IEnumerable<string> configEntries = config.ToLowerInvariant().Split(',', StringSplitOptions.RemoveEmptyEntries);
        List<string[]> weatherWithWeightEntries = configEntries.Select(kvp => kvp.Split('=', StringSplitOptions.RemoveEmptyEntries)).ToList();
        foreach (string[] weatherWithWeightEntry in weatherWithWeightEntries)
        {
            if (weatherWithWeightEntry.Length != 2)
                continue;

            NamespacedKey weatherNamespacedKey = NamespacedKey.ForceParse(weatherWithWeightEntry[0].Trim());

            string weightFactor = weatherWithWeightEntry[1].Trim();
            if (string.IsNullOrEmpty(weightFactor))
                continue;

            MatchingWeathersWithWeightAndOperationDict.Add(weatherNamespacedKey, weightFactor);
        }
    }

    public override float GetNewWeight(float currentWeight)
    {
        if (!TimeOfDay.Instance) return currentWeight;
        if (!TimeOfDay.Instance.currentLevel) return currentWeight;

        NamespacedKey currentWeatherNamespacedKey = NamespacedKey<DawnWeatherEffectInfo>.Vanilla("none");
        IEnumerable<NamespacedKey> allTags = [];
        if (TimeOfDay.Instance.currentLevel.currentWeather != LevelWeatherType.None && TimeOfDay.Instance.effects[(int)TimeOfDay.Instance.currentLevel.currentWeather].TryGetDawnInfo(out DawnWeatherEffectInfo? weatherInfo))
        {
            currentWeatherNamespacedKey = weatherInfo.TypedKey;
            allTags = weatherInfo.AllTags();
        }

        if (MatchingWeathersWithWeightAndOperationDict.TryGetValue(currentWeatherNamespacedKey, out string operationWithWeight))
        {
            return DoOperation(currentWeight, operationWithWeight);
        }

        List<NamespacedKey> validTagNamespacedKeys = new();
        foreach (NamespacedKey tagNamespacedKey in allTags)
        {
            if (MatchingWeathersWithWeightAndOperationDict.ContainsKey(tagNamespacedKey))
            {
                validTagNamespacedKeys.Add(tagNamespacedKey);
            }
        }

        if (validTagNamespacedKeys.Count > 0)
        {
            operationWithWeight = MatchingWeathersWithWeightAndOperationDict[validTagNamespacedKeys[0]];
            return DoOperation(currentWeight, operationWithWeight);
        }

        return currentWeight;
    }

    public override string GetOperation()
    {
        if (!TimeOfDay.Instance) return string.Empty;
        if (!TimeOfDay.Instance.currentLevel) return string.Empty;

        NamespacedKey currentWeatherNamespacedKey = NamespacedKey<DawnWeatherEffectInfo>.Vanilla("none");
        if (TimeOfDay.Instance.currentLevel.currentWeather != LevelWeatherType.None && TimeOfDay.Instance.effects[(int)TimeOfDay.Instance.currentLevel.currentWeather].TryGetDawnInfo(out DawnWeatherEffectInfo? weatherInfo))
        {
            currentWeatherNamespacedKey = weatherInfo.TypedKey;
        }

        if (!MatchingWeathersWithWeightAndOperationDict.TryGetValue(currentWeatherNamespacedKey, out string operationWithWeight)) return string.Empty;


        string operation = operationWithWeight[..1];
        if (operation == "+" || operation == "*" || operation == "/" || operation == "-")
        {
            return operation;
        }
        else if (float.TryParse(operation, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
        {
            return "+";
        }
        else
        {
            return string.Empty;
        }
    }
}