using System.Collections.Generic;
using System.Linq;
using Dawn;
using Dawn.Internal;

namespace Dusk.Weights.Transformers;

internal static class WeightTransformerTagLogic
{
    public static float ApplyByKeyOrTags(float currentWeight, NamespacedKey typedKey, IEnumerable<NamespacedKey> allTags, Dictionary<NamespacedKey, (MathOperation operation, float weight)> dict, System.Func<float, (MathOperation operation, float weight), float> doOperation, DebugLogSource? log = null)
    {
        if (dict.TryGetValue(typedKey, out var opWithWeight))
        {
            log?.Log($"NamespacedKey: {typedKey}");
            return doOperation(currentWeight, opWithWeight);
        }

        List<NamespacedKey> orderedMatches = new List<NamespacedKey>();
        HashSet<string> processed = new HashSet<string>();

        foreach (var tag in allTags)
        {
            if (!processed.Add(tag.Key))
                continue;

            foreach (var configuredKey in dict.Keys)
            {
                if (configuredKey.Key == tag.Key)
                {
                    orderedMatches.Add(configuredKey);
                    break;
                }
            }
        }

        orderedMatches = orderedMatches.OrderByDescending(k => dict[k].operation == MathOperation.Additive || dict[k].operation == MathOperation.Subtractive).ToList();

        foreach (var key in orderedMatches)
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
}
