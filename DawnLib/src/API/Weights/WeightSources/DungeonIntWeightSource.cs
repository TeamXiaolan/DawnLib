using System;
using System.Collections.Generic;
using Dawn.Internal;

namespace Dawn;

public sealed class DungeonIntWeightSource : WeightModifierSource<int>
{
    private readonly Func<IEnumerable<UnresolvedNamespacedWeight>> _getWeights;

    public DungeonIntWeightSource(Func<IEnumerable<UnresolvedNamespacedWeight>> getWeights)
    {
        _getWeights = getWeights;
    }

    public override void Build(WeightBuildContext context, List<IWeightModifier<int>> modifiers)
    {
        using NamespacedKeyResolver<DawnDungeonInfo> resolver = new(context.Dungeons.Values);
        foreach (UnresolvedNamespacedWeight unresolved in _getWeights())
        {
            Debuggers.Weights?.Log($"Building DungeonIntWeightSource with input {unresolved.KeyInput} and value {unresolved.Value}");
            ResolvedNamespacedWeight<DawnDungeonInfo>? resolved = resolver.ResolveWeight(unresolved);

            if (resolved == null)
                continue;

            modifiers.Add(new DungeonIntWeightModifier(resolved.Value));
        }
    }
}