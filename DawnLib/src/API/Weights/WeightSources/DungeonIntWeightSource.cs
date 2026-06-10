using System.Collections.Generic;

namespace Dawn;

public sealed class DungeonIntWeightSource : IWeightModifierSource<int>
{
    private readonly List<UnresolvedNamespacedWeight> _weights;

    public DungeonIntWeightSource(List<UnresolvedNamespacedWeight> weights)
    {
        _weights = weights;
    }

    public void Build(WeightBuildContext context, List<IWeightModifier<int>> modifiers)
    {
        using NamespacedKeyResolver<DawnDungeonInfo> resolver = new(context.Dungeons.Values);
        foreach (UnresolvedNamespacedWeight unresolved in _weights)
        {
            ResolvedNamespacedWeight<DawnDungeonInfo>? resolved = resolver.ResolveWeight(unresolved);

            if (resolved == null)
                continue;

            modifiers.Add(new DungeonIntWeightModifier(resolved.Value));
        }
    }
}