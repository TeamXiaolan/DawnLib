using System;

namespace CodeRebirthLib.ConfigManagement.Weights.Transformers;
[Serializable]
public abstract class WeightTransformer
{
    public WeightOperation Operation;

    public abstract string ToConfigString();
    public abstract void FromConfigString(string config);
    public abstract float GetNewWeight(float currentWeight);
    public float DoOperation(float currentValue, float previousValue)
    {
        if (Operation == WeightOperation.Additive)
        {
            return currentValue + previousValue;
        }
        else if (Operation == WeightOperation.Multiplicative)
        {
            return currentValue * previousValue;
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}