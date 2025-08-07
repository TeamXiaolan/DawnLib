using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeRebirthLib.ConfigManagement.Weights.Transformers;

[Serializable]
public class WeatherWeightTransformer : WeightTransformer
{
    public WeatherWeightTransformer(string weatherConfig)
    {
        FromConfigString(weatherConfig);
    }
    public Dictionary<string, string> MatchingWeathersWithWeightAndOperationDict = new();

    public override string ToConfigString()
    {
        string MatchingWeatherWithWeight = string.Join(",", MatchingWeathersWithWeightAndOperationDict.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
        return $"{MatchingWeatherWithWeight}";
    }

    public override void FromConfigString(string config)
    {
        MatchingWeathersWithWeightAndOperationDict = config.ToLowerInvariant().Split(':', StringSplitOptions.RemoveEmptyEntries).Select(part => part.Split(',')).ToDictionary(tokens => tokens[0].Trim(), tokens => tokens[1].Trim());
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