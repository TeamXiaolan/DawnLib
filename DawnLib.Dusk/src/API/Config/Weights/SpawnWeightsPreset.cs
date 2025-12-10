using System.Collections.Generic;
using System.Linq;
using Dawn;
using Dawn.Internal;
using Dusk.Weights.Transformers;
using UnityEngine;

namespace Dusk.Weights;
public class SpawnWeightsPreset : IWeighted
{
    public MoonWeightTransformer MoonSpawnWeightsTransformer { get; private set; }
    public InteriorWeightTransformer InteriorSpawnWeightsTransformer { get; private set; }
    public WeatherWeightTransformer WeatherSpawnWeightsTransformer { get; private set; }

    private int _baseWeightIncrease = 0;
    private List<WeightTransformer> SpawnWeightsTransformers => new() { MoonSpawnWeightsTransformer, InteriorSpawnWeightsTransformer, WeatherSpawnWeightsTransformer };

    public void SetupSpawnWeightsPreset(List<NamespacedConfigWeight> moonConfig, List<NamespacedConfigWeight> interiorConfig, List<NamespacedConfigWeight> weatherConfig, int baseWeightIncrease = 0)
    {
        MoonSpawnWeightsTransformer = new MoonWeightTransformer(moonConfig);
        InteriorSpawnWeightsTransformer = new InteriorWeightTransformer(interiorConfig);
        WeatherSpawnWeightsTransformer = new WeatherWeightTransformer(weatherConfig);
        _baseWeightIncrease = baseWeightIncrease;
        // `Namespace:MoonName1=+10,Namespace:MoonName2=-20,Namespace:MoonName3=*1.5`
        // `Namespace:InteriorName1=-10,Namespace:InteriorName2=+10,Namespace:InteriorName3=+300`
        // `Namespace:WeatherName1=10,Namespace:WeatherName2=*2.0,Namespace:WeatherName3=*1.5`
    }

    public int GetWeight()
    {
        float weight = 0;
        List<WeightTransformer> transformers = SpawnWeightsTransformers.OrderByDescending(x => x.GetOperation() == MathOperation.Additive || x.GetOperation() == MathOperation.Subtractive).ToList();
        foreach (WeightTransformer weightTransformer in transformers)
        {
            Debuggers.Weights?.Log($"Old Weight: {weight}");
            weight = weightTransformer.GetNewWeight(weight);
            Debuggers.Weights?.Log($"New Weight: {weight}");
        }

        return Mathf.RoundToInt(weight + _baseWeightIncrease);
    }
}