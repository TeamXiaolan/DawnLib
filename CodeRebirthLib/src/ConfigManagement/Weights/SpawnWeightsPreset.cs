using System.Collections.Generic;
using CodeRebirthLib.ConfigManagement.Weights.Transformers;
using UnityEngine;

namespace CodeRebirthLib.ConfigManagement.Weights;
// todo: better name
[CreateAssetMenu(menuName = "CodeRebirthLib/Weights/Preset", order = -20)]
public class SpawnWeightsPreset : ScriptableObject
{
    [field: SerializeField]
    public int BaseWeight { get; private set; }

    [field: SerializeField]
    public List<WeightTransformer> SpawnWeightsTransformers { get; private set;} = new();

    public void SetupSpawnWeightsPreset(int baseWeight, string presetsConfig)
    {
        SpawnWeightsTransformers.Clear();
        BaseWeight = baseWeight;
        // TODO differentiate between the different presets somehow in that one string and recreate all the transformers?
        // SpawnWeightsTransformers.Add(weightTransformer);
    }

    public float GetWeight()
    {
        float weight = BaseWeight;
        foreach (var weightTransformer in SpawnWeightsTransformers)
        {
            weight = weightTransformer.GetNewWeight(weight);
        }
        return weight;
    }
}

// my interior weight transformer = facility,manision : 0.3 : mult