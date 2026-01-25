using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Dawn;

public readonly struct SpawnWeightExtras
{
    private readonly Dictionary<string, object?>? _data;

    public static SpawnWeightExtras Empty => default;

    private SpawnWeightExtras(Dictionary<string, object?> data) => _data = data;

    private static string Canonical(NamespacedKey key) => key.ToString();

    public SpawnWeightExtras With<T>(NamespacedKey key, T value)
    {
        Dictionary<string, object?> copy = _data != null ? new Dictionary<string, object?>(_data) : [];

        copy[Canonical(key)] = value;
        return new SpawnWeightExtras(copy);
    }

    public bool TryGet<T>(NamespacedKey key, [NotNullWhen(true)] out T? value)
    {
        value = default;
        if (_data == null)
        {
            return false;
        }

        if (_data.TryGetValue(Canonical(key), out object? boxed) && boxed is T typed)
        {
            value = typed;
            return true;
        }

        return false;
    }

    public T GetOrDefault<T>(NamespacedKey key, T fallback = default!) => TryGet(key, out T? value) ? value : fallback;
}


public static class SpawnWeightExtraKeys
{
    public static readonly NamespacedKey RoutingPriceKey = NamespacedKey.From("dawn_lib", "spawn_weight_routing_price");
}

public readonly struct SpawnWeightContext(DawnMoonInfo? moon, DawnDungeonInfo? dungeon, DawnWeatherEffectInfo? weather, SpawnWeightExtras extras)
{
    public DawnMoonInfo? Moon { get; } = moon;
    public DawnDungeonInfo? Dungeon { get; } = dungeon;
    public DawnWeatherEffectInfo? Weather { get; } = weather;
    public SpawnWeightExtras Extras { get; } = extras;

    public SpawnWeightContext(DawnMoonInfo? moon, DawnDungeonInfo? dungeon, DawnWeatherEffectInfo? weather) : this(moon, dungeon, weather, SpawnWeightExtras.Empty) { }

    public SpawnWeightContext WithExtra<T>(NamespacedKey key, T value) => new(Moon, Dungeon, Weather, Extras.With(key, value));
}
