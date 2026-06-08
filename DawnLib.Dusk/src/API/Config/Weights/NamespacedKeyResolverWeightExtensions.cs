using System.Collections.Generic;
using Dawn;
using Dawn.Internal;

namespace Dusk.Weights;

public static class NamespacedKeyResolverWeightExtensions
{
    public static List<ResolvedNamespacedWeight<T>> ResolveWeights<T>(this NamespacedKeyResolver<T> resolver, IEnumerable<UnresolvedNamespacedWeight> weights) where T : INamespaced
    {
        List<ResolvedNamespacedWeight<T>> result = new();
        foreach (UnresolvedNamespacedWeight weight in weights)
        {
            ResolvedNamespacedWeight<T>? resolved = resolver.ResolveWeight(weight);
            if (resolved == null)
            {
                Debuggers.Weights?.Log($"Could not resolve weight key input '{weight.KeyInput}'.");
                continue;
            }

            result.Add(resolved.Value);
        }

        return result;
    }

        public static ResolvedNamespacedWeight<T>? ResolveWeight<T>(this NamespacedKeyResolver<T> resolver, UnresolvedNamespacedWeight weight) where T : INamespaced
    {
        if (!resolver.TryResolve(weight.KeyInput, out NamespacedKey<T>? key))
        {
            Debuggers.Weights?.Log($"Could not resolve weight key input '{weight.KeyInput}'.");
            return null;
        }

        return new ResolvedNamespacedWeight<T>(key, weight.Operation, weight.Value);
    }
}