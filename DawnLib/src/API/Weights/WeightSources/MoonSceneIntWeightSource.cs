using System;
using System.Collections.Generic;
using System.Linq;
using Dawn.Internal;

namespace Dawn;

public sealed class MoonSceneIntWeightSource : WeightModifierSource<int>
{
    private readonly Func<IEnumerable<UnresolvedNamespacedWeight>> _getWeights;

    public MoonSceneIntWeightSource(Func<IEnumerable<UnresolvedNamespacedWeight>> getWeights)
    {
        _getWeights = getWeights;
    }

    public override void Build(WeightBuildContext context, List<IWeightModifier<int>> modifiers)
    {
        using NamespacedKeyResolver<IMoonSceneInfo> resolver = new(context.Moons.Values.SelectMany(x => x.Scenes));
        foreach (UnresolvedNamespacedWeight unresolved in _getWeights())
        {
            Debuggers.Weights?.Log($"Building MoonSceneIntWeightSource with input {unresolved.KeyInput} and value {unresolved.Value}");
            ResolvedNamespacedWeight<IMoonSceneInfo>? resolved = resolver.ResolveWeight(unresolved);

            if (resolved == null)
                continue;

            modifiers.Add(new MoonSceneIntWeightModifier(resolved.Value));
        }
    }
}