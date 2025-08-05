using System;
using UnityEngine;

namespace CodeRebirthLib.ConfigManagement.Weights.Transformers;
public abstract class WeightTransformer : ScriptableObject
{
    public float Value;
    public WeightOperation Operation;

    public abstract string ToConfigString();
    public abstract void FromConfigString(string config);
    public abstract float GetNewWeight(float previousWeight);
    public float DoOperation(float previousValue)
    {
        if (Operation == WeightOperation.Additive)
        {
            return Value + previousValue;
        }
        else if (Operation == WeightOperation.Multiplicative)
        {
            return Value * previousValue;
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}