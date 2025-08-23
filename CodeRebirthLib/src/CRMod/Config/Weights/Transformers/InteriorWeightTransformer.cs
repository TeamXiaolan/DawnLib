using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CodeRebirthLib.CRMod;
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
        if (!RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.TryGetCRInfo(out CRDungeonInfo? dungeonInfo)) return currentWeight;
        if (!MatchingInteriorsWithWeightAndOperationDict.TryGetValue(dungeonInfo.TypedKey, out string operationWithWeight)) return currentWeight;
        /*foreach (NamespacedKey tag in moonInfo.tags)
        {
            Could potentially have a priority system, check all valid tags and apply the lowest weight one? or an average? but would need to account for the different operations
        }*/

        return DoOperation(currentWeight, operationWithWeight);
    }

    public override string GetOperation()
    {
        if (!RoundManager.Instance) return string.Empty;
        if (!RoundManager.Instance.dungeonGenerator) return string.Empty;
        if (!RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow) return string.Empty;
        if (!RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.TryGetCRInfo(out CRDungeonInfo? dungeonInfo)) return string.Empty;
        if (!MatchingInteriorsWithWeightAndOperationDict.TryGetValue(dungeonInfo.TypedKey, out string operationWithWeight)) return string.Empty;

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