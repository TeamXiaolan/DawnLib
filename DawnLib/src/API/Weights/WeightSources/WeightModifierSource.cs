using System.Collections.Generic;

namespace Dawn;

public abstract class WeightModifierSource<T> : IWeightModifierSource<T>
{
    public virtual void RefreshSource(WeightBuildContext context) { }
    public abstract void Build(WeightBuildContext context, List<IWeightModifier<T>> modifiers);
}