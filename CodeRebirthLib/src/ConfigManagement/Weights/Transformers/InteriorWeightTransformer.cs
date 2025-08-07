using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeRebirthLib.ConfigManagement.Weights.Transformers;
[Serializable]
public class InteriorWeightTransformer : WeightTransformer
{
    public InteriorWeightTransformer(string interiorConfig)
    {
        FromConfigString(interiorConfig);
    }

    public Dictionary<string, int> MatchingInteriorsWithWeightDict = new();

    public override string ToConfigString()
    {
        string MatchingInteriorWithWeight = string.Join(",", MatchingInteriorsWithWeightDict.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
        return $"{MatchingInteriorWithWeight} | {Operation}";
    }

    public override void FromConfigString(string config)
    {
        string[] split = config.Split('|');

        MatchingInteriorsWithWeightDict = split[0].Split(':').Select(s => s.Trim()).Select(s => s.ToLowerInvariant()).Select(s => s.Split(',')).Select(s => (s[0], int.Parse(s[1]))).ToDictionary(s => s.Item1, s => s.Item2);
        Operation = Enum.Parse<WeightOperation>(split[1].Trim());
    }

    public override float GetNewWeight(float currentWeight)
    {
        if (!RoundManager.Instance) return currentWeight;
        if (!RoundManager.Instance.dungeonGenerator) return currentWeight;
        if (!RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow) return currentWeight;
        if (!MatchingInteriorsWithWeightDict.TryGetValue(RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.name.ToLowerInvariant(), out int operationWeight))
            return currentWeight;

        return DoOperation(currentWeight, operationWeight);
    }
}