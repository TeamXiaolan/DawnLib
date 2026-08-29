using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BepInEx.Configuration;

namespace Dusk;

public class DuskConfigRegistry
{
    private readonly Dictionary<string, ConfigEntryBase> _entries = new();
    public IReadOnlyDictionary<string, ConfigEntryBase> Entries => _entries;

    internal void Register(string name, ConfigEntryBase entry)
    {
        if (!_entries.TryAdd(name, entry))
        {
            throw new InvalidOperationException($"A config with the name '{name}' has already been registered.");
        }
    }

    public ConfigEntry<T> Get<T>(string name)
    {
        if (TryGet(name, out ConfigEntry<T>? entry))
        {
            return entry;
        }

        throw new KeyNotFoundException($"Config '{name}' of type '{typeof(T).Name}' does not exist.");
    }

    public bool TryGet<T>(string name, [NotNullWhen(true)] out ConfigEntry<T>? entry)
    {
        entry = null;
        if (!_entries.TryGetValue(name, out ConfigEntryBase? config))
        {
            return false;
        }

        if (config is not ConfigEntry<T> typedConfig)
        {
            return false;
        }

        entry = typedConfig;
        return true;
    }
}