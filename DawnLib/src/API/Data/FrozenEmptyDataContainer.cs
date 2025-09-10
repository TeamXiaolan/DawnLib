using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Dawn;
public class FrozenEmptyDataContainer : IDataContainer
{
    public static FrozenEmptyDataContainer Instance { get; private set; } = new();
    private FrozenEmptyDataContainer() { }

    public bool TryGet<T>(NamespacedKey key, [NotNullWhen(true)] out T? value)
    {
        value = default;
        return false;
    }
    public T GetOrSetDefault<T>(NamespacedKey key, T defaultValue)
    {
        throw new RegistryFrozenException();
    }
    public T GetOrCreateDefault<T>(NamespacedKey key) where T : new()
    {
        throw new RegistryFrozenException();
    }
    public void Set<T>(NamespacedKey key, T value)
    {
        throw new RegistryFrozenException();
    }
    public void Remove(NamespacedKey key)
    {
        throw new RegistryFrozenException();
    }
    public void Clear()
    {
        throw new RegistryFrozenException();
    }
    public IEnumerable<NamespacedKey> Keys { get; } = [];
    public int Count => 0;
}