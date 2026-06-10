namespace Dawn;

public readonly struct WeightChannel<T>
{
    public WeightChannel(NamespacedKey key, IWeightValuePolicy<T> policy)
    {
        Key = key;
        Policy = policy;
    }

    public NamespacedKey Key { get; }

    public IWeightValuePolicy<T> Policy { get; }

    public static WeightChannel<T> From(NamespacedKey namespacedKey, IWeightValuePolicy<T> policy)
    {
        return new WeightChannel<T>(namespacedKey, policy);
    }
}