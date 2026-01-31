using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using Dawn.Internal;
using Dawn.Utils;
using Unity.Netcode;
using UnityEngine;

namespace Dawn;

[Serializable]
public class NamespacedKey : INetworkSerializable
{
    private static readonly Regex NamespacedKeyRegex = new(@"[?!.\n\t""`\[\]'-]");

    private static readonly Dictionary<string, NamespacedKey> CanonicalByFull = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, NamespacedKey> CanonicalByKey = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, List<NamespacedKey>> SmartPlaceholdersByKey = new(StringComparer.Ordinal);

    private static readonly Dictionary<char, string> NumberWords = new()
    {
        { '0', "Zero" },
        { '1', "One" },
        { '2', "Two" },
        { '3', "Three" },
        { '4', "Four" },
        { '5', "Five" },
        { '6', "Six" },
        { '7', "Seven" },
        { '8', "Eight" },
        { '9', "Nine" },
    };

    private static void PromoteSmartPlaceholders(string key, string newNamespace)
    {
        if (!SmartPlaceholdersByKey.TryGetValue(key, out List<NamespacedKey> list) || list.Count == 0)
            return;

        foreach (NamespacedKey placeholder in list)
        {
            if (placeholder._namespace == SmartMatchingNamespace)
            {
                Debuggers.NamespacedKeys?.Log($"Promoting placeholder {placeholder} to {newNamespace}");
                placeholder._namespace = newNamespace;
            }
        }

        // SmartPlaceholdersByKey.Remove(key);
    }

    /// <summary>
    /// Normalises input into a key / namespace friendly form.
    /// CSharpName = true -> PascalCase (for C# identifiers).
    /// CSharpName = false -> lower_snake_case (for keys / namespaces).
    /// Digits are converted to words (e.g., '1' -> "one") using NumberWords.
    /// </summary>
    internal static string NormalizeStringForNamespacedKey(string input, bool CSharpName)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        string cleanedString = NamespacedKeyRegex.Replace(input, string.Empty);

        StringBuilder cleanBuilder = new(cleanedString.Length);
        bool started = false;

        foreach (char character in cleanedString)
        {
            if (!started)
            {
                if (CSharpName)
                {
                    if (!char.IsLetter(character))
                        continue;
                }
                else
                {
                    if (!char.IsLetterOrDigit(character))
                        continue;
                }

                started = true;
            }

            cleanBuilder.Append(character);

            if (CSharpName)
            {
                if (cleanBuilder.Length < 3 || cleanBuilder.Length > 5)
                    continue;

                foreach (string word in NumberWords.Values)
                {
                    if (cleanBuilder.ToString().StartsWith(word, StringComparison.OrdinalIgnoreCase))
                    {
                        cleanBuilder.Remove(0, word.Length);
                    }
                }
            }
        }

        StringBuilder actualWordBuilder = new(cleanBuilder.Length);
        foreach (char character in cleanBuilder.ToString())
        {
            if (NumberWords.TryGetValue(character, out string word))
            {
                actualWordBuilder.Append(word);
            }
            else
            {
                actualWordBuilder.Append(character);
            }
        }

        string result = actualWordBuilder.ToString();

        if (CSharpName)
        {
            result = result.Replace(" ", string.Empty).Replace("_", string.Empty).ToCapitalized();
        }
        else
        {
            result = result.ToLowerInvariant().Replace(" ", "_");
        }
        return result;
    }

    private static string BuildFullKey(string @namespace, string key) => $"{@namespace}{Separator}{key}";

    private static bool ShouldReplaceCandidate(NamespacedKey existing, NamespacedKey candidate)
    {
        if (existing.Namespace == SmartMatchingNamespace && candidate.Namespace != SmartMatchingNamespace)
        {
            return true;
        }

        if (candidate.Namespace == VanillaNamespace && existing.Namespace != VanillaNamespace)
        {
            return true;
        }

        return false;
    }

    private static void Register(NamespacedKey key)
    {
        string full = BuildFullKey(key.Namespace, key.Key);

        CanonicalByFull.TryAdd(full, key);

        if (key.Namespace == SmartMatchingNamespace)
        {
            if (!SmartPlaceholdersByKey.TryGetValue(key.Key, out List<NamespacedKey> list))
            {
                list = new();
                SmartPlaceholdersByKey[key.Key] = list;
            }
            list.Add(key);

            if (!CanonicalByKey.ContainsKey(key.Key))
            {
                CanonicalByKey[key.Key] = key;
            }

            return;
        }

        if (CanonicalByKey.TryGetValue(key.Key, out NamespacedKey existing))
        {
            if (ShouldReplaceCandidate(existing, key))
            {
                CanonicalByKey[key.Key] = key;
                PromoteSmartPlaceholders(key.Key, key.Namespace);
            }
        }
        else
        {
            CanonicalByKey[key.Key] = key;
            PromoteSmartPlaceholders(key.Key, key.Namespace);
        }
    }

    private static bool TrySmartResolveByKey(string normalizedKey, [NotNullWhen(true)] out NamespacedKey? match)
    {
        match = null;
        if (CanonicalByKey.TryGetValue(normalizedKey, out NamespacedKey candidate))
        {
            match = candidate;
            return true;
        }

        return false;
    }

    public const char Separator = ':';
    public const string VanillaNamespace = "lethal_company";
    public const string SmartMatchingNamespace = "smart_matching";

    [field: SerializeField]
    private string _namespace, _key;

    public string Namespace => _namespace;
    public string Key => _key;

    protected NamespacedKey(string @namespace, string key)
    {
        _namespace = NormalizeStringForNamespacedKey(@namespace, CSharpName: false);
        _key = NormalizeStringForNamespacedKey(key, CSharpName: false);

        Register(this);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public NamespacedKey() { }

    public static NamespacedKey From(string @namespace, string key)
    {
        return new NamespacedKey(@namespace, key);
    }

    public static NamespacedKey Vanilla(string key)
    {
        return From(VanillaNamespace, key);
    }

    public static NamespacedKey Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Input cannot be null or empty.", nameof(input));
        }

        string[] parts = input.Split(Separator);
        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
        {
            throw new FormatException($"Invalid namespaced key '{input}'. Expected 'namespace{Separator}key'.");
        }

        return From(parts[0], parts[1]);
    }

    public static bool TryParse(string input, [NotNullWhen(true)] out NamespacedKey? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        string[] parts = input.Split(Separator);
        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
        {
            return false;
        }

        result = From(parts[0], parts[1]);
        return true;
    }

    public static NamespacedKey ForceParse(string input)
    {
        return ForceParse(input, useSmartMatching: false);
    }

    public static NamespacedKey ForceParse(string input, bool useSmartMatching)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Input cannot be null or empty.", nameof(input));
        }

        string[] parts = input.Split(Separator);

        if (parts.Length == 2)
        {
            return From(parts[0], parts[1]);
        }

        string rawKey = parts[0];
        string normalizedKey = NormalizeStringForNamespacedKey(rawKey, CSharpName: false);

        if (useSmartMatching)
        {
            if (TrySmartResolveByKey(normalizedKey, out var match))
            {
                return match;
            }

            return From(SmartMatchingNamespace, normalizedKey);
        }

        return From(VanillaNamespace, normalizedKey);
    }

    public override string ToString()
    {
        return BuildFullKey(Namespace, Key);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _namespace);
        serializer.SerializeValue(ref _key);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is not NamespacedKey other)
        {
            return false;
        }

        return string.Equals(Namespace, other.Namespace, StringComparison.Ordinal) && string.Equals(Key, other.Key, StringComparison.Ordinal);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 13;
            hash = hash * 17 + (Namespace?.GetHashCode() ?? 0);
            hash = hash * 17 + (Key?.GetHashCode() ?? 0);
            return hash;
        }
    }

    public NamespacedKey<T> AsTyped<T>() where T : INamespaced
    {
        return NamespacedKey<T>.From(Namespace, Key);
    }

    public bool IsVanilla() => Namespace == VanillaNamespace;
    public bool IsModded() => !IsVanilla();
}

[Serializable]
public class NamespacedKey<T> : NamespacedKey where T : INamespaced
{
    protected NamespacedKey(string @namespace, string key) : base(@namespace, key) { }
    /// <summary>
    /// Do not use. Only for NetworkSeralizables
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public NamespacedKey() { }

    public new static NamespacedKey<T> From(string @namespace, string key)
    {
        return new NamespacedKey<T>(@namespace, key);
    }

    public new static NamespacedKey<T> Vanilla(string key)
    {
        return From(VanillaNamespace, key);
    }

    public new static NamespacedKey<T> Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Input cannot be null or empty.", nameof(input));
        }

        string[] parts = input.Split(Separator);
        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
        {
            throw new FormatException($"Invalid namespaced key '{input}'. Expected 'namespace{Separator}key'.");
        }

        return From(parts[0], parts[1]);
    }
}