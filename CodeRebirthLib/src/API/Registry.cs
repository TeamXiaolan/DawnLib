using System;
using System.Collections;
using System.Collections.Generic;
using CodeRebirthLib.Internal;

namespace CodeRebirthLib;
public class RegistryFrozenException() : Exception("Registry is frozen")
{
}

public class Registry<T> : IReadOnlyDictionary<NamespacedKey<T>, T> where T : INamespaced<T>
{
    private readonly Dictionary<NamespacedKey<T>, T> _dictionary = [];

    public bool IsFrozen { get; private set; }
    public event Action BeforeFreeze = delegate { };
    public event Action OnFreeze = delegate { };

    virtual internal void Freeze()
    {
        if (IsFrozen)
            return;

        BeforeFreeze();
        IsFrozen = true;
        OnFreeze();
    }

    virtual internal void Register(T value)
    {
        if (IsFrozen)
            throw new RegistryFrozenException();

        NamespacedKey<T> key = value.TypedKey;
        if (ContainsKey(key))
            throw new ArgumentException($"'{key}' has already been added to this registry.");

        _dictionary[key] = value;
    }

    public IEnumerator<KeyValuePair<NamespacedKey<T>, T>> GetEnumerator()
    {
        return _dictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => _dictionary.Count;
    public bool ContainsKey(NamespacedKey<T> key) => _dictionary.ContainsKey(key);
    public bool TryGetValue(NamespacedKey<T> key, out T value)
    {
        return _dictionary.TryGetValue(key, out value);
    }

    public bool TryGetValue(NamespacedKey key, out T value)
    {
        return TryGetValue(key.AsTyped<T>(), out value);
    }

    public T this[NamespacedKey<T> key] => _dictionary[key];

    public IEnumerable<NamespacedKey<T>> Keys => _dictionary.Keys;
    public IEnumerable<T> Values => _dictionary.Values;
}