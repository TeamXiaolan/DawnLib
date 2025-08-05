using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CodeRebirthLib.ConfigManagement.Weights.Transformers;
[CreateAssetMenu(menuName = "CodeRebirthLib/Weights/Weather", order = -20)]
public class WeatherWeightTransformer : WeightTransformer
{
    public List<string> MatchingWeathers = new();

    public override string ToConfigString()
    {
        string matchingWeathers = string.Join(",", MatchingWeathers);
        return $"{matchingWeathers} | {Value} | {Operation}";
    }

    public override void FromConfigString(string config)
    {
        string[] split = config.Split('|');

        MatchingWeathers = split[0].Split(',').Select(s => s.Trim()).ToList();
        Value = float.Parse(split[1].Trim());
        Operation = Enum.Parse<WeightOperation>(split[2].Trim());
    }

    public override float GetNewWeight(float previousWeight)
    {
        if (!MatchingWeathers.Contains(RoundManager.Instance.currentLevel.currentWeather.ToString()))
            return previousWeight;

        return DoOperation(previousWeight);
    }
}