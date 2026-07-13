using System;
using System.Collections.Generic;

namespace Dawn;

public sealed class GlobalBaseIntSource : WeightModifierSource<int>
{
    private readonly Func<int> _getValue;

    public GlobalBaseIntSource(Func<int> getValue)
    {
        _getValue = getValue;
    }

    public override void Build(WeightBuildContext context, List<IWeightModifier<int>> modifiers)
    {
        modifiers.Add(new GlobalBaseIntModifier(_getValue()));
    }
}