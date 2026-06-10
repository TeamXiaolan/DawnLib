using System.Collections.Generic;

namespace Dawn;

public interface IWeightModifierSource<T>
{
    void Build(WeightBuildContext context, List<IWeightModifier<T>> modifiers);
}