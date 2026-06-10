namespace Dawn;

public interface IWeightModifier<T>
{
    NamespacedKey Key { get; }

    WeightModifierPhase Phase { get; }

    int Priority { get; }

    bool CanApply(WeightContext context);

    void Apply(ref T value, WeightContext context);
}