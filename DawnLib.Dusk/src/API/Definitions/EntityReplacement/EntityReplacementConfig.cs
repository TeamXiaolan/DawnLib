using System.Collections.Generic;
using BepInEx.Configuration;

namespace Dusk;

public class EntityReplacementConfig(ConfigContext section, string EntityNameReference) : DuskBaseConfig(section, EntityNameReference)
{
    public ConfigEntry<string>? MoonSpawnWeights;
    public ConfigEntry<string>? InteriorSpawnWeights;
    public ConfigEntry<string>? WeatherSpawnWeights;
    public ConfigEntry<bool>? DisableDateCheck;

    override internal List<ConfigEntryBase?> _configEntries => [
        MoonSpawnWeights,
        InteriorSpawnWeights,
        WeatherSpawnWeights,
        DisableDateCheck
    ];
}