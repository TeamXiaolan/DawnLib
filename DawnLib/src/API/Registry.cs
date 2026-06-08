using System;
using System.Collections;
using System.Collections.Generic;

namespace Dawn;

public class RegistryFrozenException() : Exception("Registry is frozen")
{
}

public class Registry<T> : IReadOnlyDictionary<NamespacedKey<T>, T> where T : INamespaced<T>
{
    protected readonly Dictionary<NamespacedKey<T>, T> _dictionary = [];

    public bool IsFrozen { get; private set; }
    [Obsolete("Use BeforeFreezeWithContext instead")]
    public event Action BeforeFreeze
    {
        add
        {
            BeforeFreezeWithContext += _ =>
            {
                value();
            };
        }
        remove => DawnPlugin.Logger.LogError("Registry.BeforeFreeze -= is not supported.");
    }

    public event Action<NamespacedKeyResolver<T>> BeforeFreezeWithContext
    {
        add
        {
            _beforeFreezeWithContext += resolver =>
            {
                try
                {
                    value(resolver);
                }
                catch (Exception exception)
                {
                    DawnPlugin.Logger.LogError($"(BeforeFreezeWithContext) An exception occured in firing an event for a registry:\n{exception}");
                }
            };
        }
        remove => DawnPlugin.Logger.LogError("Registry.BeforeFreezeWithContext -= is not supported.");
    }

    [Obsolete("Use OnFreezeWithContext instead")]
    public event Action OnFreeze
    {
        add
        {
            OnFreezeWithContext += _ =>
            {
                value();
            };
        }
        remove => DawnPlugin.Logger.LogError("Registry.OnFreeze -= is not supported.");
    }

    public event Action<NamespacedKeyResolver<T>> OnFreezeWithContext
    {
        add
        {
            _onFreezeWithContext += resolver =>
            {
                try
                {
                    value(resolver);
                }
                catch (Exception exception)
                {
                    DawnPlugin.Logger.LogError($"(OnFreezeWithContext) An exception occured in firing an event for a registry:\n{exception}");
                }
            };
        }
        remove => DawnPlugin.Logger.LogError("Registry.OnFreezeWithContext -= is not supported.");
    }

    private event Action<NamespacedKeyResolver<T>> _onFreezeWithContext = delegate { }, _beforeFreezeWithContext = delegate { };

    virtual internal void Freeze()
    {
        if (IsFrozen)
        {
            throw new RegistryFrozenException();
        }

        using (NamespacedKeyResolver<T> beforeFreezeResolver = new(Values))
        {
            _beforeFreezeWithContext(beforeFreezeResolver);
        }

        IsFrozen = true;

        foreach (T value in Values)
        {
            if (value is IRegistryEvents events)
            {
                events.OnFrozen();
            }
        }

        using (NamespacedKeyResolver<T> onFreezeResolver = new(Values))
        {
            _onFreezeWithContext(onFreezeResolver);
        }
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

public interface IRegistryEvents
{
    void OnFrozen();
}