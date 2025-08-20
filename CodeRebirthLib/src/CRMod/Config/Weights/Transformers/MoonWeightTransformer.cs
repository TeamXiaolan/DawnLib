using System;
using System.Collections.Generic;
using System.Linq;
using CodeRebirthLib.Internal;

namespace CodeRebirthLib.CRMod;

[Serializable]
public class MoonWeightTransformer : WeightTransformer
{
    public MoonWeightTransformer(string moonConfig)
    {
        if (string.IsNullOrEmpty(moonConfig))
            return;

        FromConfigString(moonConfig);
    }

    public Dictionary<string, string> MatchingMoonsWithWeightAndOperationDict = new();

    public override string ToConfigString()
    {
        if (MatchingMoonsWithWeightAndOperationDict.Count == 0)
            return string.Empty;

        string MatchingMoonWithWeight = string.Join(",", MatchingMoonsWithWeightAndOperationDict.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
        return $"{MatchingMoonWithWeight}";
    }

    public override void FromConfigString(string config)
    {
        if (string.IsNullOrEmpty(config))
            return;

        IEnumerable<string> configEntries = config.ToLowerInvariant().Split(',', StringSplitOptions.RemoveEmptyEntries);
        List<string[]> moonWithWeightEntries = configEntries.Select(kvp => kvp.Split(':', StringSplitOptions.RemoveEmptyEntries)).ToList();
        MatchingMoonsWithWeightAndOperationDict.Clear();
        foreach (string[] moonWithWeightEntry in moonWithWeightEntries)
        {
            if (moonWithWeightEntry.Length != 2)
                continue;

            string moonName = moonWithWeightEntry[0].Trim();
            string weightFactor = moonWithWeightEntry[1].Trim();
            if (string.IsNullOrEmpty(moonName) || string.IsNullOrEmpty(weightFactor))
                continue;

            Debuggers.Weights?.Log($"Adding {moonName} with weight {weightFactor} to MoonWeightTransformer");
            MatchingMoonsWithWeightAndOperationDict.Add(moonName, weightFactor);
        }
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