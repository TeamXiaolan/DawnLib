using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BepInEx.Configuration;
using Dawn;

namespace Dusk;

public static class DuskConfigExtensions
{
    public static bool TryGetConfig<T>(this Registry<DuskConfigDefinition> registry, NamespacedKey<DuskConfigDefinition> configKey, [NotNullWhen(true)] out ConfigEntry<T>? configEntry)
    {
        configEntry = null;
        if (!registry.TryGetValue(configKey, out DuskConfigDefinition? value))
        {
            configEntry = null;
            return false;
        }

        if (value.ConfigEntry is not ConfigEntry<T> typedEntry)
        {
            configEntry = null;
            return false;
        }

        configEntry = typedEntry;
        return true;
    }

    public static ConfigEntry<T> GetConfig<T>(this Registry<DuskConfigDefinition> registry, NamespacedKey<DuskConfigDefinition> configKey)
    {
        if (!registry.TryGetConfig(configKey, out ConfigEntry<T>? configEntry))
        {
            throw new KeyNotFoundException($"Config with key {configKey} not found or is not of type {typeof(T).Name}.");
        }

        return configEntry;
    }
}