using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeRebirthLib.ConfigManagement.Weights.Transformers;

[Serializable]
public class MoonWeightTransformer : WeightTransformer
{
    public MoonWeightTransformer(string moonConfig)
    {
        FromConfigString(moonConfig);
    }

    public Dictionary<string, int> MatchingMoonsWithWeightDict = new();

    public override string ToConfigString()
    {
        string MatchingMoonWithWeight = string.Join(",", MatchingMoonsWithWeightDict.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
        return $"{MatchingMoonWithWeight} | {Operation}";
    }

    public override void FromConfigString(string config)
    {
        string[] split = config.Split('|');

        MatchingMoonsWithWeightDict = split[0].Split(':').Select(s => s.Trim()).Select(s => s.ToLowerInvariant()).Select(s => s.Split(',')).Select(s => (s[0], int.Parse(s[1]))).ToDictionary(s => s.Item1, s => s.Item2);
        Operation = Enum.Parse<WeightOperation>(split[1].Trim());
    }

    public override float GetNewWeight(float currentWeight)
    {
        if (!MatchingMoonsWithWeightDict.TryGetValue(ConfigManager.GetLLLNameOfLevel(RoundManager.Instance.currentLevel.name), out int operationWeight))
            return currentWeight;

        return DoOperation(currentWeight, operationWeight);
    }
}