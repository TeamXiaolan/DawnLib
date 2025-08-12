using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeRebirthLib.CRMod.Weights.Transformers;
[Serializable]
public class InteriorWeightTransformer : WeightTransformer
{
    public InteriorWeightTransformer(string interiorConfig)
    {
        if (string.IsNullOrEmpty(interiorConfig))
            return;

        FromConfigString(interiorConfig);
    }

    public Dictionary<string, string> MatchingInteriorsWithWeightAndOperationDict = new();

    public override string ToConfigString()
    {
        string MatchingInteriorWithWeight = string.Join(",", MatchingInteriorsWithWeightAndOperationDict.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
        return $"{MatchingInteriorWithWeight}";
    }

    public override void FromConfigString(string config)
    {
        MatchingInteriorsWithWeightAndOperationDict = config.ToLowerInvariant().Split(':', StringSplitOptions.RemoveEmptyEntries).Select(part => part.Split(',')).ToDictionary(tokens => tokens[0].Trim(), tokens => tokens[1].Trim());
    }

    public override float GetNewWeight(float currentWeight)
    {
        if (!RoundManager.Instance) return currentWeight;
        if (!RoundManager.Instance.dungeonGenerator) return currentWeight;
        if (!RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow) return currentWeight;
        if (!MatchingInteriorsWithWeightAndOperationDict.TryGetValue(RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.name.ToLowerInvariant().Trim(), out string operationWithWeight))
            return currentWeight;

        return DoOperation(currentWeight, operationWithWeight);
    }

    public override string GetOperation()
    {
        if (!RoundManager.Instance) return string.Empty;
        if (!RoundManager.Instance.dungeonGenerator) return string.Empty;
        if (!RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow) return string.Empty;
        if (!MatchingInteriorsWithWeightAndOperationDict.TryGetValue(RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.name.ToLowerInvariant().Trim(), out string operationWithWeight))
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