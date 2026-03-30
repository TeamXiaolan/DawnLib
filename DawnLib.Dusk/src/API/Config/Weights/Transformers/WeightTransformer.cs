using System;
using Dawn.Internal;

namespace Dusk.Weights;

[Serializable]
public abstract class WeightTransformer<TContext>
{
    public abstract float GetNewWeight(float currentWeight, TContext context);
    public abstract MathOperation GetOperation(TContext context);

    protected float DoOperation<U>(float currentValue, U previousValueWithOperation) where U : IOperationWithValue
    {
        Debuggers.Weights?.Log($"Operation: {previousValueWithOperation.Operation}");
        Debuggers.Weights?.Log($"Value: {previousValueWithOperation.Value}");

        MathOperation operation = previousValueWithOperation.Operation;
        float previousValue = previousValueWithOperation.Value;

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
