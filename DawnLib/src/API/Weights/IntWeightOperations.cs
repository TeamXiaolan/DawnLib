using UnityEngine;

namespace Dawn;

public static class IntWeightOperations
{
    public static void Apply(ref int value, MathOperation operation, float amount)
    {
        switch (operation)
        {
            case MathOperation.Additive:
                value += Mathf.RoundToInt(amount);
                break;
            case MathOperation.Subtractive:
                value -= Mathf.RoundToInt(amount);
                break;
            case MathOperation.Multiplicative:
                value = Mathf.RoundToInt(value * amount);
                break;
            case MathOperation.Divisive:
                value = amount == 0 ? 0 : Mathf.RoundToInt(value / amount);
                break;
            default:
                throw new System.NotImplementedException($"Unknown operation {operation}");
        }
    }

    public static WeightModifierPhase GetPhase(MathOperation operation)
    {
        return operation is MathOperation.Additive or MathOperation.Subtractive
            ? WeightModifierPhase.Additive
            : WeightModifierPhase.Multiplicative;
    }
}