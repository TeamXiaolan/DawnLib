using System;
using System.Collections.Generic;
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
    public Dictionary<string, string> MatchingWeathersWithWeightAndOperationDict = new();

    public override string ToConfigString()
    {
        if (MatchingWeathersWithWeightAndOperationDict.Count == 0)
            return string.Empty;

        string MatchingWeatherWithWeight = string.Join(",", MatchingWeathersWithWeightAndOperationDict.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
        return $"{MatchingWeatherWithWeight}";
    }

    public override void FromConfigString(string config)
    {
        if (string.IsNullOrEmpty(config))
            return;

        IEnumerable<string> configEntries = config.ToLowerInvariant().Split(',', StringSplitOptions.RemoveEmptyEntries);
        List<string[]> moonWithWeightEntries = configEntries.Select(kvp => kvp.Split(':', StringSplitOptions.RemoveEmptyEntries)).ToList();
        MatchingWeathersWithWeightAndOperationDict.Clear();
        foreach (string[] moonWithWeightEntry in moonWithWeightEntries)
        {
            if (moonWithWeightEntry.Length != 2)
                continue;

            string moonName = moonWithWeightEntry[0].Trim();
            string weightFactor = moonWithWeightEntry[1].Trim();
            if (string.IsNullOrEmpty(moonName) || string.IsNullOrEmpty(weightFactor))
                continue;

            MatchingWeathersWithWeightAndOperationDict.Add(moonName, weightFactor);
        }
    }

    public override float GetNewWeight(float currentWeight)
    {
        if (!RoundManager.Instance) return currentWeight;
        if (!RoundManager.Instance.currentLevel) return currentWeight;
        if (!MatchingWeathersWithWeightAndOperationDict.TryGetValue(RoundManager.Instance.currentLevel.currentWeather.ToString().ToLowerInvariant().Trim(), out string operationWithWeight))
            return currentWeight;

        return DoOperation(currentWeight, operationWithWeight);
    }

    public override string GetOperation()
    {
        if (!RoundManager.Instance) return string.Empty;
        if (!RoundManager.Instance.currentLevel) return string.Empty;
        if (!MatchingWeathersWithWeightAndOperationDict.TryGetValue(RoundManager.Instance.currentLevel.currentWeather.ToString().ToLowerInvariant().Trim(), out string operationWithWeight))
            return string.Empty;

        // first character is the operation, get that as string?
        string operation = operationWithWeight[..1];
        if (int.TryParse(operation, out _)) // if no operation provided, default to `+`
        {
            return "+";
        }
        else if (operation == "+")
        {
            return "+";
        }
        else if (operation == "*")
        {
            return "*";
        }
        else if (operation == "-")
        {
            return "-";
        }
        else if (operation == "/")
        {
            return "/";
        }
        else
        {
            return string.Empty;
        }
    }
}