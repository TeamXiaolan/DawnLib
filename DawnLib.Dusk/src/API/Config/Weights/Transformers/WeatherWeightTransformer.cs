using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Dawn.Internal;

namespace Dusk.Weights.Transformers;

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
        Debuggers.Weights?.Log($"MatchingWeatherWithWeight: {MatchingWeatherWithWeight}");
        return $"{MatchingWeatherWithWeight}";
    }

    public override void FromConfigString(string config)
    {
        MatchingWeathersWithWeightAndOperationDict.Clear();
        List<string> configEntries = new();
        foreach (string entry in config.ToLowerInvariant().Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            configEntries.Add(entry.Trim().Replace(" ", "_"));
        }
        List<string[]> weatherWithWeightEntries = configEntries.Select(kvp => kvp.Split('=', StringSplitOptions.RemoveEmptyEntries)).ToList();
        if (weatherWithWeightEntries.Count == 0)
        {
            DuskPlugin.Logger.LogWarning($"Invalid weather weight config: {config}");
            DuskPlugin.Logger.LogWarning($"Expected Format: <Namespace>:<Key>=<Operation><Value> | i.e. weather_registry:snowfall=+20");
            return;
        }

        foreach (string[] weatherWithWeightEntry in weatherWithWeightEntries)
        {
            if (weatherWithWeightEntry.Length != 2)
            {
                DuskPlugin.Logger.LogWarning($"Invalid weather weight entry: {string.Join(",", weatherWithWeightEntry)} from config: {config}");
                DuskPlugin.Logger.LogWarning($"Expected Format: <Namespace>:<Key>=<Operation><Value> | i.e. weather_registry:snowfall=+20");
                continue;
            }

            NamespacedKey weatherNamespacedKey = NamespacedKey.ForceParse(weatherWithWeightEntry[0].Trim());

            string weightFactor = weatherWithWeightEntry[1].Trim();
            if (string.IsNullOrEmpty(weightFactor))
            {
                DuskPlugin.Logger.LogWarning($"Invalid weather weight entry: {string.Join(",", weatherWithWeightEntry)} from config: {config}");
                DuskPlugin.Logger.LogWarning($"Entry did not have a provided weight factor, defaulting to 0.");
                weightFactor = "+0";
            }

            MatchingWeathersWithWeightAndOperationDict.Add(weatherNamespacedKey, weightFactor);
        }
    }

    public override float GetNewWeight(float currentWeight)
    {
        if (!TimeOfDay.Instance) return currentWeight;
        if (!TimeOfDay.Instance.currentLevel) return currentWeight;

        NamespacedKey currentWeatherNamespacedKey = NamespacedKey<DawnWeatherEffectInfo>.Vanilla("none");
        IEnumerable<NamespacedKey> allTags = [];
        if (TimeOfDay.Instance.currentLevel.currentWeather != LevelWeatherType.None)
        {
            DawnWeatherEffectInfo weatherInfo = TimeOfDay.Instance.effects[(int)TimeOfDay.Instance.currentLevel.currentWeather].GetDawnInfo();
            currentWeatherNamespacedKey = weatherInfo.TypedKey;
            allTags = weatherInfo.AllTags();
        }

        if (MatchingWeathersWithWeightAndOperationDict.TryGetValue(currentWeatherNamespacedKey, out string operationWithWeight))
        {
            return DoOperation(currentWeight, operationWithWeight);
        }

        List<NamespacedKey> orderedAndValidTagNamespacedKeys = new();
        foreach (NamespacedKey tagNamespacedKey in allTags)
        {
            if (MatchingWeathersWithWeightAndOperationDict.ContainsKey(tagNamespacedKey))
            {
                orderedAndValidTagNamespacedKeys.Add(tagNamespacedKey);
            }
        }

        orderedAndValidTagNamespacedKeys = orderedAndValidTagNamespacedKeys.OrderBy(x => Operation(MatchingWeathersWithWeightAndOperationDict[x]) == "+" || Operation(MatchingWeathersWithWeightAndOperationDict[x]) == "-").ToList();
        foreach (NamespacedKey namespacedKey in orderedAndValidTagNamespacedKeys)
        {
            operationWithWeight = MatchingWeathersWithWeightAndOperationDict[namespacedKey];
            currentWeight = DoOperation(currentWeight, operationWithWeight);
        }

        return currentWeight;
    }

    public override string GetOperation()
    {
        if (!TimeOfDay.Instance) return string.Empty;
        if (!TimeOfDay.Instance.currentLevel) return string.Empty;

        NamespacedKey currentWeatherNamespacedKey = NamespacedKey<DawnWeatherEffectInfo>.Vanilla("none");
        if (TimeOfDay.Instance.currentLevel.currentWeather != LevelWeatherType.None)
        {
            DawnWeatherEffectInfo weatherInfo = TimeOfDay.Instance.effects[(int)TimeOfDay.Instance.currentLevel.currentWeather].GetDawnInfo();
            currentWeatherNamespacedKey = weatherInfo.TypedKey;
        }

        if (!MatchingWeathersWithWeightAndOperationDict.TryGetValue(currentWeatherNamespacedKey, out string operationWithWeight)) return string.Empty;

        return Operation(operationWithWeight[..1]);
    }
}