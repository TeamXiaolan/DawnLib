using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BepInEx.Configuration;
using Dawn;

namespace Sundown;

public static class SundownConfigExtensions
{
    public static bool TryGetConfig<T>(this Registry<SundownConfigDefinition> registry, NamespacedKey<SundownConfigDefinition> configKey, [NotNullWhen(true)] out ConfigEntry<T>? configEntry)
    {
        configEntry = null;
        if (!registry.TryGetValue(configKey, out SundownConfigDefinition? value))
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

    public static ConfigEntry<T> GetConfig<T>(this Registry<SundownConfigDefinition> registry, NamespacedKey<SundownConfigDefinition> configKey)
    {
        if (!registry.TryGetConfig(configKey, out ConfigEntry<T>? configEntry))
        {
            throw new KeyNotFoundException($"Config with key {configKey} not found or is not of type {typeof(T).Name}.");
        }

        return configEntry;
    }
}