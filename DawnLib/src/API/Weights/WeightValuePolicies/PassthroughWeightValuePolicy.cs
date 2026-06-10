namespace Dawn;

public sealed class PassthroughWeightValuePolicy<T> : IWeightValuePolicy<T>
{
    public static readonly PassthroughWeightValuePolicy<T> Default = new();

    public T InitialValue => default!;

    public T Finalize(T value, WeightContext context)
    {
        return value;
    }
}