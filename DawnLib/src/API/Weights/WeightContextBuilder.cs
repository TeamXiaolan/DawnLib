using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Dawn;

public sealed class WeightContextBuilder
{
    private readonly Dictionary<NamespacedKey, object?> _values = new();

    public WeightContextBuilder(WeightQuery query)
    {
        Query = query.ResolveGameState();
    }

    public WeightQuery Query { get; }

    public void Set<T>(NamespacedKey key, T value)
    {
        _values[key] = value;
    }

    public bool TryGet<T>(NamespacedKey key, [NotNullWhen(true)] out T? value)
    {
        value = default;

        if (!_values.TryGetValue(key, out object? boxed))
            return false;

        if (boxed is not T typed)
            return false;

        value = typed;
        return true;
    }

    public WeightContext Build()
    {
        return new WeightContext(Query, new Dictionary<NamespacedKey, object?>(_values));
    }
}