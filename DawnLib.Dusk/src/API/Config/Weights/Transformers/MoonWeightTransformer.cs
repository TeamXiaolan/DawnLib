using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Dawn.Internal;

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
        Debuggers.Weights?.Log($"MatchingMoonWithWeight: {MatchingMoonWithWeight}");
        return $"{MatchingMoonWithWeight}";
    }

    public override void FromConfigString(string config)
    {
        MatchingMoonsWithWeightAndOperationDict.Clear();
        List<string> configEntries = new();
        foreach (string entry in config.ToLowerInvariant().Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            configEntries.Add(entry.Trim().Replace(" ", "_"));
        }
        List<string[]> moonWithWeightEntries = configEntries.Select(kvp => kvp.Split('=', StringSplitOptions.RemoveEmptyEntries)).ToList();
        if (moonWithWeightEntries.Count == 0)
        {
            DuskPlugin.Logger.LogWarning($"Invalid moon weight config: {config}");
            DuskPlugin.Logger.LogWarning($"Expected Format: <Namespace>:<Key>=<Operation><Value> | i.e. magic_wesleysmod:trite=+20");
            return;
        }

        foreach (string[] moonWithWeightEntry in moonWithWeightEntries)
        {
            if (moonWithWeightEntry.Length != 2)
            {
                DuskPlugin.Logger.LogWarning($"Invalid moon weight entry: {string.Join(",", moonWithWeightEntry)} from config: {config}");
                DuskPlugin.Logger.LogWarning($"Expected Format: <Namespace>:<Key>=<Operation><Value> | i.e. magic_wesleysmod:trite=+20");
                continue;
            }

            NamespacedKey moonNamespacedKey = NamespacedKey.ForceParse(moonWithWeightEntry[0].Trim());

            string weightFactor = moonWithWeightEntry[1].Trim();
            if (string.IsNullOrEmpty(weightFactor))
            {
                DuskPlugin.Logger.LogWarning($"Invalid moon weight entry: {string.Join(",", moonWithWeightEntry)} from config: {config}");
                DuskPlugin.Logger.LogWarning($"Entry did not have a provided weight factor, defaulting to 0.");
                weightFactor = "+0";
            }

            MatchingMoonsWithWeightAndOperationDict.Add(moonNamespacedKey, weightFactor);
        }
    }

    public override float GetNewWeight(float currentWeight)
    {
        if (!RoundManager.Instance) return currentWeight;
        if (!RoundManager.Instance.currentLevel) return currentWeight;
        DawnMoonInfo moonInfo = RoundManager.Instance.currentLevel.GetDawnInfo();
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
        DawnMoonInfo moonInfo = RoundManager.Instance.currentLevel.GetDawnInfo();
        if (!MatchingMoonsWithWeightAndOperationDict.TryGetValue(moonInfo.TypedKey, out string operationWithWeight)) return string.Empty;

        return Operation(operationWithWeight[0..1]);
    }
}