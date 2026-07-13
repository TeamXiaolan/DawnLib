using System.Collections.Generic;

namespace Dawn;

public interface IWeightModifierSource<T>
{
    void RefreshSource(WeightBuildContext context);
    void Build(WeightBuildContext context, List<IWeightModifier<T>> modifiers);
}