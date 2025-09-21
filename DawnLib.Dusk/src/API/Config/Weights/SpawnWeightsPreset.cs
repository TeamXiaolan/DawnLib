using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Dusk.Weights.Transformers;
using UnityEngine;

namespace Dusk.Weights;
public class SpawnWeightsPreset : IWeighted
{
    public MoonWeightTransformer MoonSpawnWeightsTransformer { get; private set; } = new(string.Empty);
    public InteriorWeightTransformer InteriorSpawnWeightsTransformer { get; private set; } = new(string.Empty);
    public WeatherWeightTransformer WeatherSpawnWeightsTransformer { get; private set; } = new(string.Empty);

    private List<WeightTransformer> SpawnWeightsTransformers => new() { MoonSpawnWeightsTransformer, InteriorSpawnWeightsTransformer, WeatherSpawnWeightsTransformer };

    public void SetupSpawnWeightsPreset(string moonConfig, string interiorConfig, string weatherConfig)
    {
        MoonSpawnWeightsTransformer = new MoonWeightTransformer(moonConfig);
        InteriorSpawnWeightsTransformer = new InteriorWeightTransformer(interiorConfig);
        WeatherSpawnWeightsTransformer = new WeatherWeightTransformer(weatherConfig);
        // `MoonName1:+10,MoonName2:-20,MoonName3:*1.5`
        // `InteriorName1:-10,InteriorName2:+10,InteriorName3:+300`
        // `WeatherName1:10,WeatherName2:*2.0,WeatherName3:*1.5`
    }

    public int GetWeight()
    {
        float weight = 0;
        SpawnWeightsTransformers.OrderBy(x => x.GetOperation() == "+" || x.GetOperation() == "-").ToList();
        foreach (WeightTransformer weightTransformer in SpawnWeightsTransformers)
        {
            weight = weightTransformer.GetNewWeight(weight);
        }

        return Mathf.FloorToInt(weight);
    }
}