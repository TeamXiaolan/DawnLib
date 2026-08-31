using System;
using System.Collections.Generic;
using Dawn.Internal;

namespace Dawn;

public sealed class MoonIntWeightSource : WeightModifierSource<int>
{
    private readonly Func<IEnumerable<UnresolvedNamespacedWeight>> _getWeights;

    public MoonIntWeightSource(Func<IEnumerable<UnresolvedNamespacedWeight>> getWeights)
    {
        _getWeights = getWeights;
    }

    public override void Build(WeightBuildContext context, List<IWeightModifier<int>> modifiers)
    {
        using NamespacedKeyResolver<DawnMoonInfo> resolver = new(context.Moons.Values);
        foreach (UnresolvedNamespacedWeight unresolved in _getWeights())
        {
            Debuggers.Weights?.Log($"Building MoonIntWeightSource with input {unresolved.KeyInput} and value {unresolved.Value}");
            ResolvedNamespacedWeight<DawnMoonInfo>? resolved = resolver.ResolveWeight(unresolved);
            if (resolved == null)
                continue;

            modifiers.Add(new MoonIntWeightModifier(resolved.Value));
        }
    }
}