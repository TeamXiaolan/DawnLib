using System.Collections.Generic;

namespace Dawn;

public sealed class GlobalBaseIntSource : IWeightModifierSource<int>
{
    private readonly int _value;

    public GlobalBaseIntSource(int value)
    {
        _value = value;
    }

    public void Build(WeightBuildContext context, List<IWeightModifier<int>> modifiers)
    {
        modifiers.Add(new GlobalBaseIntModifier(_value));
    }
}