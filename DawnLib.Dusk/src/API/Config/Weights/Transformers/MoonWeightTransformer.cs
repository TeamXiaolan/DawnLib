using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;

namespace Dusk.Weights.Transformers;

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
        if (!RoundManager.Instance.currentLevel.TryGetDawnInfo(out DawnMoonInfo? moonInfo)) return currentWeight;
        if (MatchingMoonsWithWeightAndOperationDict.TryGetValue(moonInfo.TypedKey, out string operationWithWeight))
        {
            return DoOperation(currentWeight, operationWithWeight);
        }

        List<NamespacedKey> orderedAndValidTagNamespacedKeys = new();
        foreach (NamespacedKey tagNamespacedKey in moonInfo.AllTags())
        {
            if (MatchingMoonsWithWeightAndOperationDict.ContainsKey(tagNamespacedKey))
            {
                orderedAndValidTagNamespacedKeys.Add(tagNamespacedKey);
            }
        }

        orderedAndValidTagNamespacedKeys = orderedAndValidTagNamespacedKeys.OrderBy(x => Operation(MatchingMoonsWithWeightAndOperationDict[x]) == "+" || Operation(MatchingMoonsWithWeightAndOperationDict[x]) == "-").ToList();
        foreach (NamespacedKey namespacedKey in orderedAndValidTagNamespacedKeys)
        {
            operationWithWeight = MatchingMoonsWithWeightAndOperationDict[namespacedKey];
            currentWeight = DoOperation(currentWeight, operationWithWeight);
        }

        return currentWeight;
    }

    public override string GetOperation()
    {
        if (!RoundManager.Instance) return string.Empty;
        if (!RoundManager.Instance.currentLevel) return string.Empty;
        if (!RoundManager.Instance.currentLevel.TryGetDawnInfo(out DawnMoonInfo? moonInfo)) return string.Empty;
        if (!MatchingMoonsWithWeightAndOperationDict.TryGetValue(moonInfo.TypedKey, out string operationWithWeight)) return string.Empty;

        return Operation(operationWithWeight[..1]);
    }
}