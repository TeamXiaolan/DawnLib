using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Dawn;
public interface IDataContainer
{
    bool TryGet<T>(NamespacedKey key, [NotNullWhen(true)] out T? value);
    T GetOrSetDefault<T>(NamespacedKey key, T defaultValue);
    T GetOrCreateDefault<T>(NamespacedKey key) where T : new();
    void Set<T>(NamespacedKey key, T value);
    void Remove(NamespacedKey key);
    void Clear();

    IEnumerable<NamespacedKey> Keys { get; }
    int Count { get; }

    void MarkDirty();
    IDisposable CreateEditContext();
}