using System.Collections.Generic;

namespace Dawn;

public sealed class MoonIntWeightSource : IWeightModifierSource<int>
{
    private readonly List<UnresolvedNamespacedWeight> _weights;

    public MoonIntWeightSource(List<UnresolvedNamespacedWeight> weights)
    {
        _weights = weights;
    }

    public void Build(WeightBuildContext context, List<IWeightModifier<int>> modifiers)
    {
        using NamespacedKeyResolver<DawnMoonInfo> resolver = new(context.Moons.Values);
        foreach (UnresolvedNamespacedWeight unresolved in _weights)
        {
            ResolvedNamespacedWeight<DawnMoonInfo>? resolved = resolver.ResolveWeight(unresolved);

            if (resolved == null)
                continue;

            modifiers.Add(new MoonIntWeightModifier(resolved.Value));
        }
    }
}