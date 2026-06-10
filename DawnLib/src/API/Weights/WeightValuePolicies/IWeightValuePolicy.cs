namespace Dawn;

public interface IWeightValuePolicy<T>
{
    T InitialValue { get; }

    T Finalize(T value, WeightContext context);
}