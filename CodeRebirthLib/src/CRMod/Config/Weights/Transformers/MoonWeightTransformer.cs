using System;
using System.Collections.Generic;
using System.Globalization;
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
        if (!RoundManager.Instance.currentLevel.TryGetCRInfo(out CRMoonInfo? moonInfo)) return currentWeight;
        if (!MatchingMoonsWithWeightAndOperationDict.TryGetValue(moonInfo.TypedKey, out string operationWithWeight)) return currentWeight;
        /*foreach (NamespacedKey tag in moonInfo.tags)
        {
            Could potentially have a priority system, check all valid tags and apply the lowest weight one? or an average? but would need to account for the different operations
        }*/

        return DoOperation(currentWeight, operationWithWeight);
    }

    public override string GetOperation()
    {
        if (!RoundManager.Instance) return string.Empty;
        if (!RoundManager.Instance.currentLevel) return string.Empty;
        if (!RoundManager.Instance.currentLevel.TryGetCRInfo(out CRMoonInfo? moonInfo)) return string.Empty;
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