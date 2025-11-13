using System;
using System.Collections.Generic;
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
        MatchingInteriorsWithWeightAndOperationDict.Clear();
        List<string> configEntries = new();
        foreach (string entry in config.ToLowerInvariant().Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            configEntries.Add(entry.Trim().Replace(" ", "_"));
        }
        List<string[]> interiorWithWeightEntries = configEntries.Select(kvp => kvp.Split('=', StringSplitOptions.RemoveEmptyEntries)).ToList();
        if (interiorWithWeightEntries.Count == 0)
        {
            DuskPlugin.Logger.LogWarning($"Invalid interior weight config: {config}");
            DuskPlugin.Logger.LogWarning($"Expected Format: <Namespace>:<Key>=<Operation><Value> | i.e. magic_wesleysmod:museuminteriorflow=+20");
            return;
        }

        foreach (string[] interiorWithWeightEntry in interiorWithWeightEntries)
        {
            if (interiorWithWeightEntry.Length != 2)
            {
                DuskPlugin.Logger.LogWarning($"Invalid interior weight entry: {string.Join(",", interiorWithWeightEntry)} from config: {config}");
                DuskPlugin.Logger.LogWarning($"Expected Format: <Namespace>:<Key>=<Operation><Value> | i.e. magic_wesleysmod:museuminteriorflow=+20");
                continue;
            }

            NamespacedKey interiorNamespacedKey = NamespacedKey.ForceParse(interiorWithWeightEntry[0].Trim());

            string weightFactor = interiorWithWeightEntry[1].Trim();
            if (string.IsNullOrEmpty(weightFactor))
            {
                DuskPlugin.Logger.LogWarning($"Invalid moon weight entry: {string.Join(",", interiorWithWeightEntry)} from config: {config}");
                DuskPlugin.Logger.LogWarning($"Entry did not have a provided weight factor, defaulting to 0.");
                weightFactor = "+0";
            }

            MatchingInteriorsWithWeightAndOperationDict.Add(interiorNamespacedKey, weightFactor);
        }
    }

    public override float GetNewWeight(float currentWeight)
    {
        if (!RoundManager.Instance) return currentWeight;
        if (!RoundManager.Instance.dungeonGenerator) return currentWeight;
        if (!RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow) return currentWeight;
        DawnDungeonInfo dungeonInfo = RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.GetDawnInfo();
        if (MatchingInteriorsWithWeightAndOperationDict.TryGetValue(dungeonInfo.TypedKey, out string operationWithWeight))
        {
            return DoOperation(currentWeight, operationWithWeight);
        }

        List<NamespacedKey> orderedAndValidTagNamespacedKeys = new();
        foreach (NamespacedKey tagNamespacedKey in dungeonInfo.AllTags())
        {
            foreach (NamespacedKey interiorNamespacedKey in MatchingInteriorsWithWeightAndOperationDict.Keys)
            {
                if (interiorNamespacedKey.Key == tagNamespacedKey.Key)
                {
                    orderedAndValidTagNamespacedKeys.Add(tagNamespacedKey);
                    break;
                }
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
        DawnDungeonInfo dungeonInfo = RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.GetDawnInfo();
        if (!MatchingInteriorsWithWeightAndOperationDict.TryGetValue(dungeonInfo.TypedKey, out string operationWithWeight)) return string.Empty;

        return Operation(operationWithWeight[0..1]);
    }
}