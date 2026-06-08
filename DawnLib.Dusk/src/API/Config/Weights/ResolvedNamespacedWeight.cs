using Dawn;

namespace Dusk.Weights;

public readonly struct ResolvedNamespacedWeight<T> : IOperationWithValue where T : INamespaced
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