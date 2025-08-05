using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CodeRebirthLib.ConfigManagement.Weights.Transformers;
[CreateAssetMenu(menuName = "CodeRebirthLib/Weights/Interior", order = -20)]
public class InteriorWeightTransformer : WeightTransformer
{
    public List<string> MatchingInteriors = new();

    public override string ToConfigString()
    {
        string matchingInteriors = string.Join(",", MatchingInteriors);
        return $" {matchingInteriors} : {Value} : {Operation} |";
    }

    public override void FromConfigString(string config)
    {
        string[] split = config.Split(':');

        MatchingInteriors = split[0].Split(',').Select(s => s.Trim()).Select(s => s.ToLowerInvariant()).ToList();
        Value = float.Parse(split[1].Trim());
        Operation = Enum.Parse<WeightOperation>(split[2].Trim());
    }

    public override float GetNewWeight(float previousWeight)
    {
        if (!MatchingInteriors.Contains(RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.name))
            return previousWeight;

        return DoOperation(previousWeight);
    }
}