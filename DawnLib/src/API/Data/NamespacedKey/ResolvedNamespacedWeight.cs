namespace Dawn;

public readonly struct ResolvedNamespacedWeight<T> : IOperationWithValue where T : INamespaced
{
    public NamespacedKey<T> Key { get; }

    public MathOperation Operation { get; }

    public float Value { get; }

    public ResolvedNamespacedWeight(NamespacedKey<T> key, MathOperation operation, float value)
    {
        Key = key;
        Operation = operation;
        Value = value;
    }
}