using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Dawn.Internal;

namespace Dusk.Weights;

internal static class WeightTransformerTagLogic
{
    public static bool TryApplyByKey<T, U>(float currentWeight, T typedKey, Dictionary<T, U> dict, Func<float, U, float> doOperation, out float newWeight, DebugLogSource? log = null) where U : IOperationWithValue
    {
        if (dict.TryGetValue(typedKey, out U opWithWeight))
        {
            log?.Log($"NamespacedKey: {typedKey}");
            newWeight = doOperation(currentWeight, opWithWeight);
            return true;
        }

        newWeight = currentWeight;
        return false;
    }

    public static float ApplyByTags<U>(float currentWeight, IEnumerable<NamespacedKey> allTags, Dictionary<NamespacedKey, U> dict, Func<float, U, float> doOperation, DebugLogSource? log = null) where U : IOperationWithValue
    {
        List<NamespacedKey> orderedMatches = GetOrderedTagMatches(allTags, dict);

        foreach (NamespacedKey key in orderedMatches)
        {
            log?.Log($"NamespacedKey: {key}");
            currentWeight = doOperation(currentWeight, dict[key]);
        }

        if (orderedMatches.Count == 0)
        {
            return currentWeight;
        }

        return currentWeight / orderedMatches.Count;
    }

    private static List<NamespacedKey> GetOrderedTagMatches<U>(IEnumerable<NamespacedKey> allTags, Dictionary<NamespacedKey, U> dict) where U : IOperationWithValue
    {
        List<NamespacedKey> matches = new List<NamespacedKey>();
        HashSet<string> processed = new HashSet<string>();

        foreach (NamespacedKey tag in allTags)
        {
            if (!processed.Add(tag.Key))
                continue;

            foreach (NamespacedKey configuredKey in dict.Keys)
            {
                if (configuredKey.Key == tag.Key)
                {
                    matches.Add(configuredKey);
                    break;
                }
            }
        }

        return matches
            .OrderByDescending(k =>
                dict[k].Operation == MathOperation.Additive ||
                dict[k].Operation == MathOperation.Subtractive)
            .ToList();
    }
}