namespace Dawn;

public static class FloatWeightOperations
{
    public static void Apply(ref float value, MathOperation operation, float amount)
    {
        switch (operation)
        {
            case MathOperation.Additive:
                value += amount;
                break;
            case MathOperation.Subtractive:
                value -= amount;
                break;
            case MathOperation.Multiplicative:
                value = value * amount;
                break;
            case MathOperation.Divisive:
                value = amount == 0 ? 0 : value / amount;
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