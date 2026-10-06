using System.Collections.Generic;
using Dawn.Internal;

namespace Dawn;

public static class NamespacedKeyResolverWeightExtensions
{
    public static List<ResolvedNamespacedWeight> ResolveWeights<T>(this NamespacedKeyResolver<T> resolver, IEnumerable<UnresolvedNamespacedWeight> weights) where T : INamespaced
    {
        List<ResolvedNamespacedWeight> result = new();
        foreach (UnresolvedNamespacedWeight weight in weights)
        {
            ResolvedNamespacedWeight? resolved = resolver.ResolveWeight(weight);
            if (resolved == null)
            {
                Debuggers.Weights?.Log($"Could not resolve weight key input '{weight.KeyInput}'.");
                continue;
            }

            result.Add(resolved.Value);
        }

        return result;
    }

    public static ResolvedNamespacedWeight? ResolveWeight<T>(this NamespacedKeyResolver<T> resolver, UnresolvedNamespacedWeight weight) where T : INamespaced
    {
        if (!resolver.TryResolve(weight.KeyInput, out NamespacedKey? key))
        {
            return null;
        }

        return new ResolvedNamespacedWeight(key, weight.Operation, weight.Value);
    }
}