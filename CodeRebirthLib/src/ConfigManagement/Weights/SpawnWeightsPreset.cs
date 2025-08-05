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

// my interior weight transformer = facility,manision | 0.3 | mult