using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CodeRebirthLib.ConfigManagement.Weights.Transformers;
[CreateAssetMenu(menuName = "CodeRebirthLib/Weights/Moon", order = -20)]
public class MoonWeightTransformer : WeightTransformer
{
    public List<string> MatchingMoons = new();

    public override string ToConfigString()
    {
        string matchingMoons = string.Join(",", MatchingMoons);
        return $"{matchingMoons} | {Value} | {Operation}";
    }

    public override void FromConfigString(string config)
    {
        string[] split = config.Split('|');

        MatchingMoons = split[0].Split(',').Select(s => s.Trim()).ToList();
        Value = float.Parse(split[1].Trim());
        Operation = Enum.Parse<WeightOperation>(split[2].Trim());
    }

    public override float GetNewWeight(float previousWeight)
    {
        if (!MatchingMoons.Contains(ConfigManager.GetLLLNameOfLevel(RoundManager.Instance.currentLevel.name)))
            return previousWeight;

        return DoOperation(previousWeight);
    }
}