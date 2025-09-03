using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Dawn.Dusk;

[Serializable]
public class MoonWeightTransformer : WeightTransformer
{
    public MoonWeightTransformer(string moonConfig)
    {
        if (string.IsNullOrEmpty(moonConfig))
            return;

        FromConfigString(moonConfig);
    }

    public Dictionary<NamespacedKey, string> MatchingMoonsWithWeightAndOperationDict = new();

    public override string ToConfigString()
    {
        if (MatchingMoonsWithWeightAndOperationDict.Count == 0)
            return string.Empty;

        string MatchingMoonWithWeight = string.Join(",", MatchingMoonsWithWeightAndOperationDict.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        return $"{MatchingMoonWithWeight}";
    }

    public override void FromConfigString(string config)
    {
        if (string.IsNullOrEmpty(config))
            return;

        MatchingMoonsWithWeightAndOperationDict.Clear();
        IEnumerable<string> configEntries = config.ToLowerInvariant().Split(',', StringSplitOptions.RemoveEmptyEntries);
        List<string[]> moonWithWeightEntries = configEntries.Select(kvp => kvp.Split('=', StringSplitOptions.RemoveEmptyEntries)).ToList();
        foreach (string[] moonWithWeightEntry in moonWithWeightEntries)
        {
            if (moonWithWeightEntry.Length != 2)
                continue;

            NamespacedKey moonNamespacedKey = NamespacedKey.ForceParse(moonWithWeightEntry[0].Trim());

            string weightFactor = moonWithWeightEntry[1].Trim();
            if (string.IsNullOrEmpty(weightFactor))
                continue;

            MatchingMoonsWithWeightAndOperationDict.Add(moonNamespacedKey, weightFactor);
        }
    }

    public override float GetNewWeight(float currentWeight)
    {
        if (!RoundManager.Instance) return currentWeight;
        if (!RoundManager.Instance.currentLevel) return currentWeight;
        if (!RoundManager.Instance.currentLevel.TryGetCRInfo(out DawnMoonInfo? moonInfo)) return currentWeight;
        if (MatchingMoonsWithWeightAndOperationDict.TryGetValue(moonInfo.TypedKey, out string operationWithWeight))
        {
            return DoOperation(currentWeight, operationWithWeight);
        }

        List<NamespacedKey> validTagNamespacedKeys = new();
        foreach (NamespacedKey tagNamespacedKey in moonInfo.AllTags())
        {
            if (MatchingMoonsWithWeightAndOperationDict.ContainsKey(tagNamespacedKey))
            {
                validTagNamespacedKeys.Add(tagNamespacedKey);
            }
        }

        if (validTagNamespacedKeys.Count > 0)
        {
            operationWithWeight = MatchingMoonsWithWeightAndOperationDict[validTagNamespacedKeys[0]];
            return DoOperation(currentWeight, operationWithWeight);
        }

        return currentWeight;
    }

    public override string GetOperation()
    {
        if (!RoundManager.Instance) return string.Empty;
        if (!RoundManager.Instance.currentLevel) return string.Empty;
        if (!RoundManager.Instance.currentLevel.TryGetCRInfo(out DawnMoonInfo? moonInfo)) return string.Empty;
        if (!MatchingMoonsWithWeightAndOperationDict.TryGetValue(moonInfo.TypedKey, out string operationWithWeight)) return string.Empty;

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