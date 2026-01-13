using System;
using Dawn.Internal;

namespace Dusk.Weights.Transformers;

[Serializable]
public abstract class WeightTransformer<TContext>
{
    public abstract float GetNewWeight(float currentWeight, TContext context);
    public abstract MathOperation GetOperation(TContext context);

    protected float DoOperation(float currentValue, (MathOperation operation, float value) previousValueWithOperation)
    {
        Debuggers.Weights?.Log($"Operation: {previousValueWithOperation.operation}");
        Debuggers.Weights?.Log($"Value: {previousValueWithOperation.value}");

        MathOperation operation = previousValueWithOperation.operation;
        float previousValue = previousValueWithOperation.value;

        return operation switch
        {
            MathOperation.Additive => currentValue + previousValue,
            MathOperation.Multiplicative => currentValue * previousValue,
            MathOperation.Subtractive => currentValue - previousValue,
            MathOperation.Divisive => previousValue == 0 ? 0 : currentValue / previousValue,
            _ => throw new NotImplementedException($"Unknown operation: {operation} with WeightTransformer {this}.")
        };
    }
}
