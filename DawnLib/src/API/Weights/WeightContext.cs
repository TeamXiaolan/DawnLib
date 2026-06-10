using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Dawn;

public sealed class WeightContext
{
    private readonly Dictionary<NamespacedKey, object?> _values;

    internal WeightContext(WeightQuery query, Dictionary<NamespacedKey, object?> values)
    {
        Query = query;
        _values = values;
    }

    public WeightQuery Query { get; }

    public NamespacedKey Channel => Query.Channel;

    public object? Owner => Query.Owner;

    public object? Subject => Query.Subject;

    public DawnMoonInfo? Moon => Query.Moon;

    public DawnDungeonInfo? Dungeon => Query.Dungeon;

    public DawnWeatherEffectInfo? Weather => Query.Weather;

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

    public T GetOrDefault<T>(NamespacedKey key, T fallback = default!)
    {
        return TryGet(key, out T? value) ? value : fallback;
    }
}