using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BepInEx.AssemblyPublicizer;
using Newtonsoft.Json.Linq;

namespace Dawn;
public class DataContainer : IDataContainer
{
    protected Dictionary<NamespacedKey, object> dictionary = [];

    public bool Has(NamespacedKey key)
    {
        return dictionary.ContainsKey(key);
    }

    public bool TryGet<T>(NamespacedKey key, [NotNullWhen(true)] out T? value)
    {
        if (dictionary.TryGetValue(key, out var obj))
        {
            if (obj is JToken token)
            {
                value = token.ToObject<T>()!;
                return true;
            }

            if (obj is T casted)
            {
                value = casted;
                return true;
            }

            // this is dumb. newtonsoft json sometimes stores numbers as longs, which dont cast correctly?
            if (typeof(T) == typeof(int) && obj is long longValue)
            {
                value = (T)(object)(int)(longValue); // compiler funny bussines
                return true;
            }
            if (typeof(T) == typeof(long) && obj is int intValue)
            {
                value = (T)(object)(long)(intValue); // compiler funny bussines
                return true;
            }

            throw new InvalidCastException($"type of '{key}' is {obj.GetType().Name} which can not be {typeof(T).Name}");
        }
        value = default;
        return false;
    }

    public T GetOrSetDefault<T>(NamespacedKey key, T defaultValue)
    {
        if (TryGet(key, out T? value))
            return value;

        Set(key, defaultValue);
        return defaultValue;
    }

    public T GetOrCreateDefault<T>(NamespacedKey key) where T : new()
    {
        if (TryGet(key, out T? value))
            return value;

        value = new T();
        Set(key, value);
        return value;
    }

    public virtual void Set<T>(NamespacedKey key, T value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        dictionary[key] = value;
    }

    public virtual void Remove(NamespacedKey key)
    {
        dictionary.Remove(key);
    }

    public virtual void Clear()
    {
        dictionary.Clear();
    }

    public IEnumerable<NamespacedKey> Keys => dictionary.Keys;
    public int Count => dictionary.Count;
    public virtual void MarkDirty() { } // marking dirty does nothing here
    public virtual IDisposable CreateEditContext() => new NoOpDisposable();
    
    class NoOpDisposable : IDisposable
    {
        public void Dispose()
        { }
    }
}