using System.Collections.Generic;

namespace Dawn;

public sealed class MoonBaseIntSource : IWeightModifierSource<int>
{
    private readonly NamespacedKey<DawnMoonInfo> _moonKey;
    private readonly int _value;

    public MoonBaseIntSource(NamespacedKey<DawnMoonInfo> moonKey, int value)
    {
        _moonKey = moonKey;
        _value = value;
    }

    public void Build(WeightBuildContext context, List<IWeightModifier<int>> modifiers)
    {
        modifiers.Add(new MoonBaseIntModifier(_moonKey, _value));
    }
}