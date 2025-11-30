using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using Dawn.Utils;
using Unity.Netcode;
using UnityEngine;

namespace Dawn;

[Serializable]
public class NamespacedKey : INetworkSerializable
{
    private static readonly Regex NamespacedKeyRegex = new(@"[?!.\n\t""`\[\]'-]");
    private static readonly List<NamespacedKey> AllNamespacedKeys = new();

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

    internal static string NormalizeStringForNamespacedKey(string input, bool CSharpName)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        string cleanedString = NamespacedKeyRegex.Replace(input, string.Empty);

        StringBuilder cleanBuilder = new StringBuilder(cleanedString.Length);
        bool foundAllBeginningDigits = false;
        foreach (char character in cleanedString)
        {
            if (!foundAllBeginningDigits && (char.IsDigit(character) || character == ' '))
            {
                continue;
            }
            foundAllBeginningDigits = true;
            cleanBuilder.Append(character);
        }

        StringBuilder actualWordBuilder = new StringBuilder(cleanBuilder.Length);
        foreach (char character in cleanBuilder.ToString())
        {
            if (NumberWords.TryGetValue(character, out var word))
                actualWordBuilder.Append(word);
            else
                actualWordBuilder.Append(character);
        }

        string result = actualWordBuilder.ToString();
        if (CSharpName)
        {
            result = result.Replace(" ", "");
            result = result.Replace("_", "");
            result = result.ToCapitalized();
        }
        else
        {
            result = result.ToLowerInvariant().Replace(" ", "_");
        }
        return result;
    }

    public const char Separator = ':';
    public const string VanillaNamespace = "lethal_company";

    [field: SerializeField]
    private string _namespace, _key;

    public string Namespace => _namespace;
    public string Key => _key;

    protected NamespacedKey(string @namespace, string key)
    {
        _namespace = @namespace;
        _key = key;
        AllNamespacedKeys.Add(this);
    }

    /// <summary>
    /// Do not use. Only for NetworkSeralizables
    /// </summary>
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
        string[] parts = input.Split(Separator);
        return From(parts[0], parts[1]);
    }

    public static bool TryParse(string input, [NotNullWhen(true)] out NamespacedKey? result)
    {
        result = null;
        string[] parts = input.Split(Separator);
        if (parts.Length != 2)
        {
            return false;
        }

        result = From(parts[0], parts[1]);
        return true;
    }

    public static NamespacedKey ForceParse(string input, bool useSmartMatching = false)
    {
        string[] parts = input.Split(Separator);
        if (parts.Length == 1)
        {
            if (useSmartMatching)
            {
                foreach (NamespacedKey namespacedKey in AllNamespacedKeys)
                {
                    if (namespacedKey.Key == parts[0])
                        return namespacedKey;
                }
            }
            parts = [VanillaNamespace, parts[0]];
        }
        return From(parts[0], parts[1]);
    }

    public override string ToString()
    {
        return $"{Namespace}{Separator}{Key}";
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _namespace);
        serializer.SerializeValue(ref _key);
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) // TODO??
            return false;

        NamespacedKey other = (NamespacedKey)obj;
        return Namespace == other.Namespace && Key == other.Key;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 13;
            hash = hash * 17 + Namespace.GetHashCode();
            hash = hash * 17 + Key.GetHashCode();
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

// todo: is there anyway to not do the duplication for the From/Vanilla/Parse methods? or am i stuck with it because generics
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
        string[] parts = input.Split(Separator);
        return From(parts[0], parts[1]);
    }
}