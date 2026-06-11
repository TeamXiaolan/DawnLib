using System.Collections.Generic;
using System.Linq;

namespace Dawn;

public sealed class MoonSceneIntWeightSource : IWeightModifierSource<int>
{
    private readonly List<UnresolvedNamespacedWeight> _weights;

    public MoonSceneIntWeightSource(List<UnresolvedNamespacedWeight> weights)
    {
        _weights = weights;
    }

    public void Build(WeightBuildContext context, List<IWeightModifier<int>> modifiers)
    {
        using NamespacedKeyResolver<IMoonSceneInfo> resolver = new(context.Moons.Values.SelectMany(x => x.Scenes));
        foreach (UnresolvedNamespacedWeight unresolved in _weights)
        {
            ResolvedNamespacedWeight<IMoonSceneInfo>? resolved = resolver.ResolveWeight(unresolved);

            if (resolved == null)
                continue;

            modifiers.Add(new MoonSceneIntWeightModifier(resolved.Value));
        }
    }
}