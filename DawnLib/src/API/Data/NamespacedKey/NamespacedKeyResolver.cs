using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dawn.Internal;

namespace Dawn;

public class NamespacedKeyResolver : IDisposable
{
    public enum MatchType
    {
        ExactKey,
        CompactKey,
        Fuzzy
    };

    private readonly List<NamespacedKey> _keys = new();

    private readonly Dictionary<string, NamespacedKey> _fullLookup = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<NamespacedKey>> _keyLookup = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<NamespacedKey>> _compactKeyLookup = new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, NamespacedKey?> _cache = new(StringComparer.OrdinalIgnoreCase);

    private readonly bool _allowFuzzyMatching;

    public NamespacedKeyResolver(IEnumerable<NamespacedKey> keys, bool allowFuzzyMatching = true)
    {
        _allowFuzzyMatching = allowFuzzyMatching;

        foreach (NamespacedKey key in keys)
        {
            AddKey(key);
        }
    }

    public List<NamespacedKey> Resolve(IEnumerable<string> inputs)
    {
        List<NamespacedKey> resolved = new();

        foreach (string input in inputs)
        {
            if (TryResolve(input, out NamespacedKey? key))
            {
                resolved.Add(key);
            }
        }

        return resolved;
    }

    public bool TryResolve(string input, [NotNullWhen(true)] out NamespacedKey? key)
    {
        key = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        string cacheKey = input.Trim();

        if (_cache.TryGetValue(cacheKey, out key))
        {
            return key != null;
        }

        key = ResolveInternal(cacheKey);
        _cache[cacheKey] = key;

        if (key == null)
        {
            Debuggers.NamespacedKeys?.Log($"Could not resolve NamespacedKey input '{input}'.");
            return false;
        }

        return true;
    }

    private NamespacedKey? ResolveInternal(string input)
    {
        if (TryResolveFullKey(input, out NamespacedKey? fullKey))
        {
            return fullKey;
        }

        string normalizedInput = NamespacedKey.NormalizeStringForNamespacedKey(input, CSharpName: false);
        string compactInput = Compact(normalizedInput);

        if (_keyLookup.TryGetValue(normalizedInput, out List<NamespacedKey>? exactKeyMatches))
        {
            return PickBestMatch(input, exactKeyMatches, MatchType.ExactKey);
        }

        if (_compactKeyLookup.TryGetValue(compactInput, out List<NamespacedKey>? compactMatches))
        {
            return PickBestMatch(input, compactMatches, MatchType.CompactKey);
        }

        if (_allowFuzzyMatching)
        {
            return TryResolveFuzzy(input, normalizedInput, compactInput);
        }

        return null;
    }

    private bool TryResolveFullKey(string input, out NamespacedKey? key)
    {
        key = null;

        if (!input.Contains(NamespacedKey.Separator))
        {
            return false;
        }

        if (!NamespacedKey.TryParse(input, out NamespacedKey? parsed))
        {
            return false;
        }

        if (_fullLookup.TryGetValue(parsed.ToString(), out NamespacedKey? exactMatch))
        {
            key = exactMatch;
            return true;
        }

        if (!_allowFuzzyMatching)
        {
            return false;
        }

        List<NamespacedKey> namespaceMatches = _keys
            .Where(x => x.Namespace == parsed.Namespace)
            .ToList();

        NamespacedKey? fuzzyMatch = GetBestFuzzyMatch(parsed.Key, namespaceMatches);
        if (fuzzyMatch == null)
        {
            return false;
        }

        DawnPlugin.Logger.LogError(
            $"Fuzzy NamespacedKey match used. Input '{input}' resolved to '{fuzzyMatch}'. \n" +
            $"This should be changed to the exact key '{fuzzyMatch}'.");

        key = fuzzyMatch;
        return true;
    }

    private NamespacedKey? TryResolveFuzzy(string rawInput, string normalizedInput, string compactInput)
    {
        NamespacedKey? match = GetBestFuzzyMatch(normalizedInput, compactInput, _keys);

        if (match == null)
        {
            return null;
        }

        DawnPlugin.Logger.LogError(
            $"Fuzzy NamespacedKey match used. Input '{rawInput}' resolved to '{match}'.\n" +
            $"This should be changed to the exact name '{match.Key}' or full key '{match}'.");

        return match;
    }

    private NamespacedKey? GetBestFuzzyMatch(string normalizedInput, List<NamespacedKey> keys)
    {
        return GetBestFuzzyMatch(normalizedInput, Compact(normalizedInput), keys);
    }

    private NamespacedKey? GetBestFuzzyMatch(string normalizedInput, string compactInput, List<NamespacedKey> keys)
    {
        int bestDistance = int.MaxValue;
        List<NamespacedKey> bestMatches = new();
        foreach (NamespacedKey key in keys)
        {
            string keyOnly = key.Key;
            string compactKey = Compact(keyOnly);

            int distance = Math.Min(LevenshteinDistance(normalizedInput, keyOnly), LevenshteinDistance(compactInput, compactKey));
            int maxDistance = GetMaxDistance(normalizedInput, keyOnly);

            if (distance > maxDistance)
            {
                continue;
            }

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestMatches.Clear();
                bestMatches.Add(key);
                continue;
            }

            if (distance == bestDistance)
            {
                bestMatches.Add(key);
            }
        }

        if (bestMatches.Count == 0)
        {
            return null;
        }

        return PickBestMatch(normalizedInput, bestMatches, MatchType.Fuzzy);
    }

    private NamespacedKey PickBestMatch(string input, List<NamespacedKey> matches, MatchType matchType)
    {
        if (matches.Count == 1)
        {
            return matches[0];
        }

        NamespacedKey selected = matches[0];
        Debuggers.NamespacedKeys?.Log(BuildMultipleMatchWarning(input, selected, matches, matchType));
        return selected;
    }

    private void AddKey(NamespacedKey key)
    {
        _keys.Add(key);

        _fullLookup[key.ToString()] = key;

        AddToLookup(_keyLookup, key.Key, key);
        AddToLookup(_compactKeyLookup, Compact(key.Key), key);
    }

    private static void AddToLookup(Dictionary<string, List<NamespacedKey>> lookup, string lookupKey, NamespacedKey key)
    {
        if (!lookup.TryGetValue(lookupKey, out List<NamespacedKey>? keys))
        {
            keys = new List<NamespacedKey>();
            lookup[lookupKey] = keys;
        }

        keys.Add(key);
    }

    private static string BuildMultipleMatchWarning(string input, NamespacedKey selected, List<NamespacedKey> matches, MatchType matchType)
    {
        return $"NamespacedKey input '{input}' matched multiple keys using {matchType} matching.\n" +
                $"Using '{selected}'. Other matches: {string.Join(", ", matches.Where(x => !x.Equals(selected)))}";
    }

    private static string Compact(string value)
    {
        return value
            .Replace("_", string.Empty)
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty)
            .ToLowerInvariant();
    }

    private static int GetMaxDistance(string a, string b)
    {
        int longest = Math.Max(a.Length, b.Length);

        if (longest <= 5)
        {
            return 1;
        }

        if (longest <= 12)
        {
            return 2;
        }

        return 3;
    }

    private static int LevenshteinDistance(string a, string b)
    {
        int[,] distances = new int[a.Length + 1, b.Length + 1];

        for (int i = 0; i <= a.Length; i++)
        {
            distances[i, 0] = i;
        }

        for (int j = 0; j <= b.Length; j++)
        {
            distances[0, j] = j;
        }

        for (int i = 1; i <= a.Length; i++)
        {
            for (int j = 1; j <= b.Length; j++)
            {
                int cost = a[i - 1] == b[j - 1] ? 0 : 1;

                distances[i, j] = Math.Min(
                    Math.Min(
                        distances[i - 1, j] + 1,
                        distances[i, j - 1] + 1),
                    distances[i - 1, j - 1] + cost);
            }
        }

        return distances[a.Length, b.Length];
    }

    public void Dispose()
    {
        _keys.Clear();
        _fullLookup.Clear();
        _keyLookup.Clear();
        _compactKeyLookup.Clear();
        _cache.Clear();
    }
}

public class NamespacedKeyResolver<T> : IDisposable where T : INamespaced
{
    private readonly NamespacedKeyResolver _resolver;

    public NamespacedKeyResolver(IEnumerable<T> values, bool allowFuzzyMatching = true)
    {
        _resolver = new NamespacedKeyResolver(CollectResolvableKeys(values), allowFuzzyMatching);
    }

    private static IEnumerable<NamespacedKey> CollectResolvableKeys(IEnumerable<T> values)
    {
        HashSet<NamespacedKey> seen = new();

        foreach (T value in values)
        {
            if (seen.Add(value.Key))
            {
                yield return value.Key;
            }

            if (value is ITaggable tagged)
            {
                foreach (NamespacedKey tag in tagged.AllTags())
                {
                    if (tag == null)
                    {
                        continue;
                    }

                    if (seen.Add(tag))
                    {
                        yield return tag;
                    }
                }
            }

            if (value is IAdditionalResolvableKeys additionalKeysProvider)
            {
                foreach (NamespacedKey additionalKey in additionalKeysProvider.AdditionalResolvableKeys())
                {
                    if (additionalKey == null)
                    {
                        continue;
                    }

                    if (seen.Add(additionalKey))
                    {
                        yield return additionalKey;
                    }
                }
            }
        }
    }

    public List<NamespacedKey<T>> Resolve(IEnumerable<string> inputs)
    {
        List<NamespacedKey<T>> resolved = new();

        foreach (string input in inputs)
        {
            if (TryResolve(input, out NamespacedKey<T>? key))
            {
                resolved.Add(key);
            }
        }

        return resolved;
    }

    public bool TryResolve(string input, [NotNullWhen(true)] out NamespacedKey<T>? key)
    {
        key = null;

        if (!_resolver.TryResolve(input, out NamespacedKey? resolved))
        {
            return false;
        }

        key = resolved.AsTyped<T>();
        return true;
    }

    public void Dispose()
    {
        _resolver.Dispose();
    }
}