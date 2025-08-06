using System.Collections.Generic;
using System.Linq;
using CodeRebirthLib.ConfigManagement.Weights.Transformers;
using UnityEngine;

namespace CodeRebirthLib.ConfigManagement.Weights;
// todo: better name
[CreateAssetMenu(menuName = "CodeRebirthLib/Weights/Preset", order = -20)]
public class SpawnWeightsPreset : ScriptableObject
{
    [field: SerializeField]
    public MoonWeightTransformer MoonSpawnWeightsTransformer { get; private set;}

    [field: SerializeField]
    public InteriorWeightTransformer InteriorSpawnWeightsTransformer { get; private set;}

    [field: SerializeField]
    public WeatherWeightTransformer WeatherSpawnWeightsTransformer { get; private set;}

    private List<WeightTransformer> SpawnWeightsTransformers => new() {MoonSpawnWeightsTransformer, InteriorSpawnWeightsTransformer, WeatherSpawnWeightsTransformer};

    public void SetupSpawnWeightsPreset(string moonConfig, string interiorConfig, string weatherConfig)
    {
        MoonSpawnWeightsTransformer = new MoonWeightTransformer(moonConfig);
        InteriorSpawnWeightsTransformer = new InteriorWeightTransformer(interiorConfig);
        WeatherSpawnWeightsTransformer = new WeatherWeightTransformer(weatherConfig);
        // `MoonName1:10,MoonName2:20,MoonName3:30 | Additive`
        // `InteriorName1:-10,InteriorName2:10,InteriorName3:300 | Additive`
        // `WeatherName1:10,WeatherName2:2.0,WeatherName3:1.5 | Multiplicative`
        // TODO differentiate between the different presets somehow in that one string and recreate all the transformers?
    }

    public float GetWeight()
    {
        float weight = 0;
        SpawnWeightsTransformers.OrderBy(x => x.Operation == WeightOperation.Additive).ToList();
        foreach (var weightTransformer in SpawnWeightsTransformers)
        {
            weight = weightTransformer.GetNewWeight(weight);
        }
        return weight;
    }
}