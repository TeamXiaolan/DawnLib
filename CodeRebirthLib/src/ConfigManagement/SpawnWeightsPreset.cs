using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeRebirthLib.ConfigManagement;
// todo: better name
[CreateAssetMenu(menuName = "CodeRebirthLib/Weights/Preset", order = 99238958)]
public class SpawnWeightsPreset : ScriptableObject
{
    [field: SerializeField]
    public int BaseWeight { get; private set; }
    
    List<WeightTransformer> _spawnWeights;
}

public abstract class WeightTransformer : ScriptableObject
{
    public abstract string ToConfigString();
    public abstract void FromConfigString(string config);
}

[CreateAssetMenu(menuName = "CodeRebirthLib/Weights/Interior", order = 99238958)]
public class InteriorWeightTransformer : WeightTransformer
{
    public List<string> MatchingINteriorus;
    
    public override string ToConfigString()
    {
        throw new NotImplementedException();
    }
    public override void FromConfigString(string config)
    {
        throw new NotImplementedException();
    }
}

// my interior weight transformer = facility,manision | 0.3 | mult

[CreateAssetMenu(menuName = "CodeRebirthLib/Weights/weathre", order = 99238958)]
public class weqwathe : WeightTransformer
{

    public override string ToConfigString()
    {
        throw new NotImplementedException();
    }
    public override void FromConfigString(string config)
    {
        throw new NotImplementedException();
    }
}