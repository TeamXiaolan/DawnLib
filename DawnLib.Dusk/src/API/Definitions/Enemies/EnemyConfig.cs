using System.Collections.Generic;
using BepInEx.Configuration;

namespace Dusk;

public class EnemyConfig(ConfigContext section, string EntityNameReference) : DuskBaseConfig(section, EntityNameReference)
{
    public ConfigEntry<string>? MoonSpawnWeights;
    public ConfigEntry<string>? InteriorSpawnWeights;
    public ConfigEntry<string>? WeatherSpawnWeights;
    public ConfigEntry<float>? PowerLevel;
    public ConfigEntry<int>? MaxSpawnCount;

    override internal List<ConfigEntryBase?> _configEntries => [
        MoonSpawnWeights,
        InteriorSpawnWeights,
        WeatherSpawnWeights,
        PowerLevel,
        MaxSpawnCount
    ];
}