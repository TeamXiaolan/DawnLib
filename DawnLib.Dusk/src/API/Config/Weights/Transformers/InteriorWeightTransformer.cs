using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dawn;
using Dawn.Internal;

namespace Dusk.Weights.Transformers;
[Serializable]
public class InteriorWeightTransformer : WeightTransformer
{
    public InteriorWeightTransformer(string interiorConfig)
    {
        if (string.IsNullOrEmpty(interiorConfig))
            return;

        FromConfigString(interiorConfig);
    }

    public Dictionary<NamespacedKey, string> MatchingInteriorsWithWeightAndOperationDict = new();

    public override string ToConfigString()
    {
        if (MatchingInteriorsWithWeightAndOperationDict.Count == 0)
            return string.Empty;

        string MatchingInteriorWithWeight = string.Join(",", MatchingInteriorsWithWeightAndOperationDict.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        Debuggers.Weights?.Log($"MatchingInteriorWithWeight: {MatchingInteriorWithWeight}");
        return $"{MatchingInteriorWithWeight}";
    }

    public override void FromConfigString(string config)
    {
        if (string.IsNullOrEmpty(config))
            return;

        MatchingInteriorsWithWeightAndOperationDict.Clear();
        IEnumerable<string> configEntries = config.ToLowerInvariant().Split(',', StringSplitOptions.RemoveEmptyEntries);
        List<string[]> interiorWithWeightEntries = configEntries.Select(kvp => kvp.Split('=', StringSplitOptions.RemoveEmptyEntries)).ToList();
        foreach (string[] interiorWithWeightEntry in interiorWithWeightEntries)
        {
            if (interiorWithWeightEntry.Length != 2)
                continue;

            NamespacedKey interiorNamespacedKey = NamespacedKey.ForceParse(interiorWithWeightEntry[0].Trim());

            string weightFactor = interiorWithWeightEntry[1].Trim();
            if (string.IsNullOrEmpty(weightFactor))
                continue;

            MatchingInteriorsWithWeightAndOperationDict.Add(interiorNamespacedKey, weightFactor);
        }
    }

    public override float GetNewWeight(float currentWeight)
    {
        if (!RoundManager.Instance) return currentWeight;
        if (!RoundManager.Instance.dungeonGenerator) return currentWeight;
        if (!RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow) return currentWeight;
        if (!RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.TryGetDawnInfo(out DawnDungeonInfo? dungeonInfo)) return currentWeight;
        if (MatchingInteriorsWithWeightAndOperationDict.TryGetValue(dungeonInfo.TypedKey, out string operationWithWeight))
        {
            return DoOperation(currentWeight, operationWithWeight);
        }

        List<NamespacedKey> orderedAndValidTagNamespacedKeys = new();
        foreach (NamespacedKey tagNamespacedKey in dungeonInfo.AllTags())
        {
            if (MatchingInteriorsWithWeightAndOperationDict.ContainsKey(tagNamespacedKey))
            {
                orderedAndValidTagNamespacedKeys.Add(tagNamespacedKey);
            }
        }

        orderedAndValidTagNamespacedKeys = orderedAndValidTagNamespacedKeys.OrderBy(x => Operation(MatchingInteriorsWithWeightAndOperationDict[x]) == "+" || Operation(MatchingInteriorsWithWeightAndOperationDict[x]) == "-").ToList();
        foreach (NamespacedKey namespacedKey in orderedAndValidTagNamespacedKeys)
        {
            operationWithWeight = MatchingInteriorsWithWeightAndOperationDict[namespacedKey];
            currentWeight = DoOperation(currentWeight, operationWithWeight);
        }

        return currentWeight;
    }

    public override string GetOperation()
    {
        if (!RoundManager.Instance) return string.Empty;
        if (!RoundManager.Instance.dungeonGenerator) return string.Empty;
        if (!RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow) return string.Empty;
        if (!RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.TryGetDawnInfo(out DawnDungeonInfo? dungeonInfo)) return string.Empty;
        if (!MatchingInteriorsWithWeightAndOperationDict.TryGetValue(dungeonInfo.TypedKey, out string operationWithWeight)) return string.Empty;

        return Operation(operationWithWeight[..1]);
    }
}