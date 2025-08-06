using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeRebirthLib.ConfigManagement.Weights.Transformers;
[Serializable]
public class WeatherWeightTransformer : WeightTransformer
{
    public WeatherWeightTransformer(string weatherConfig)
    {
        FromConfigString(weatherConfig);
    }
    public Dictionary<string, int> MatchingWeathersWithWeightDict = new();

    public override string ToConfigString()
    {
        string MatchingWeatherWithWeight = string.Join(",", MatchingWeathersWithWeightDict.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
        return $"{MatchingWeatherWithWeight} | {Operation}";
    }

    public override void FromConfigString(string config)
    {
        string[] split = config.Split('|');

        MatchingWeathersWithWeightDict = split[0].Split(':').Select(s => s.Trim()).Select(s => s.ToLowerInvariant()).Select(s => s.Split(',')).Select(s => (s[0], int.Parse(s[1]))).ToDictionary(s => s.Item1, s => s.Item2);
        Operation = Enum.Parse<WeightOperation>(split[1].Trim());
    }

    public override float GetNewWeight(float currentWeight)
    {
        if (!MatchingWeathersWithWeightDict.TryGetValue(RoundManager.Instance.currentLevel.currentWeather.ToString().ToLowerInvariant(), out int operationWeight))
            return currentWeight;

        return DoOperation(currentWeight, operationWeight);
    }
}