using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeRebirthLib.ConfigManagement.Weights.Transformers;

[Serializable]
public class MoonWeightTransformer : WeightTransformer
{
    public MoonWeightTransformer(string moonConfig)
    {
        FromConfigString(moonConfig);
    }

    public Dictionary<string, string> MatchingMoonsWithWeightAndOperationDict = new();

    public override string ToConfigString()
    {
        string MatchingMoonWithWeight = string.Join(",", MatchingMoonsWithWeightAndOperationDict.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
        return $"{MatchingMoonWithWeight}";
    }

    public override void FromConfigString(string config)
    {
        MatchingMoonsWithWeightAndOperationDict = config.ToLowerInvariant().Split(':', StringSplitOptions.RemoveEmptyEntries).Select(part => part.Split(',')).ToDictionary(tokens => tokens[0].Trim(), tokens => tokens[1].Trim());
    }

    public override float GetNewWeight(float currentWeight)
    {
        if (!RoundManager.Instance) return currentWeight;
        if (!RoundManager.Instance.currentLevel) return currentWeight;
        if (!MatchingMoonsWithWeightAndOperationDict.TryGetValue(ConfigManager.GetLLLNameOfLevel(RoundManager.Instance.currentLevel.name), out string operationWithWeight))
            return currentWeight;

        return DoOperation(currentWeight, operationWithWeight);
    }

    public override string GetOperation()
    {
        if (!RoundManager.Instance) return string.Empty;
        if (!RoundManager.Instance.currentLevel) return string.Empty;
        if (!MatchingMoonsWithWeightAndOperationDict.TryGetValue(ConfigManager.GetLLLNameOfLevel(RoundManager.Instance.currentLevel.name), out string operationWithWeight))
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