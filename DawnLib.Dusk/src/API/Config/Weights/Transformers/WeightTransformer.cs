using System;
using Dawn;
using Dawn.Internal;

namespace Dusk.Weights.Transformers;

[Serializable]
public abstract class WeightTransformer
{
    public abstract float GetNewWeight(float currentWeight);
    public abstract MathOperation GetOperation();

    public float DoOperation(float currentValue, (MathOperation operation, float value) previousValueWithOperation)
    {
        Debuggers.Weights?.Log($"Operation: {previousValueWithOperation.operation}");
        Debuggers.Weights?.Log($"Value: {previousValueWithOperation.value}");

        MathOperation operation = previousValueWithOperation.operation;
        float previousValue = previousValueWithOperation.value;

        if (operation == MathOperation.Additive)
        {
            return currentValue + previousValue;
        }
        else if (operation == MathOperation.Multiplicative)
        {
            return currentValue * previousValue;
        }
        else if (operation == MathOperation.Subtractive)
        {
            return currentValue - previousValue;
        }
        else if (operation == MathOperation.Divisive)
        {
            if (previousValue == 0)
            {
                return 0;
            }
            return currentValue / previousValue;
        }
        else
        {
            DawnPlugin.Logger.LogError($"Unknown operation: {operation} with WeightTransformer {this}.");
            throw new NotImplementedException();
        }
    }
}