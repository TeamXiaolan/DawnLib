using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CodeRebirthLib.CRMod;

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

        NamespacedKey currentWeatherNamespacedKey = NamespacedKey<CRWeatherEffectInfo>.Vanilla("none");
        if (TimeOfDay.Instance.currentLevel.currentWeather != LevelWeatherType.None && TimeOfDay.Instance.effects[(int)TimeOfDay.Instance.currentLevel.currentWeather].TryGetCRInfo(out CRWeatherEffectInfo? weatherInfo))
        {
            currentWeatherNamespacedKey = weatherInfo.TypedKey;
        }

        if (!MatchingWeathersWithWeightAndOperationDict.TryGetValue(currentWeatherNamespacedKey, out string operationWithWeight)) return currentWeight;
        /*foreach (NamespacedKey tag in moonInfo.tags)
        {
            Could potentially have a priority system, check all valid tags and apply the lowest weight one? or an average? but would need to account for the different operations
        }*/

        return DoOperation(currentWeight, operationWithWeight);
    }

    public override string GetOperation()
    {
        if (!TimeOfDay.Instance) return string.Empty;
        if (!TimeOfDay.Instance.currentLevel) return string.Empty;

        NamespacedKey currentWeatherNamespacedKey = NamespacedKey<CRWeatherEffectInfo>.Vanilla("none");
        if (TimeOfDay.Instance.currentLevel.currentWeather != LevelWeatherType.None && TimeOfDay.Instance.effects[(int)TimeOfDay.Instance.currentLevel.currentWeather].TryGetCRInfo(out CRWeatherEffectInfo? weatherInfo))
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