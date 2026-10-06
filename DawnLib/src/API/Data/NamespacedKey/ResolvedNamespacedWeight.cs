namespace Dawn;

public readonly struct ResolvedNamespacedWeight : IOperationWithValue
{
    public NamespacedKey Key { get; }

    public MathOperation Operation { get; }

    public float Value { get; }

    public ResolvedNamespacedWeight(NamespacedKey key, MathOperation operation, float value)
    {
        Key = key;
        Operation = operation;
        Value = value;
    }
}