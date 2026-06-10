using System.Collections.Generic;

namespace Dawn;

public sealed class CompositeIntWeightSource : IWeightModifierSource<int>
{
    private readonly List<IWeightModifierSource<int>> _sources = new();

    public CompositeIntWeightSource Add(IWeightModifierSource<int> source)
    {
        _sources.Add(source);
        return this;
    }

    public void Build(WeightBuildContext context, List<IWeightModifier<int>> modifiers)
    {
        foreach (IWeightModifierSource<int> source in _sources)
        {
            source.Build(context, modifiers);
        }
    }
}