using System.Collections.Generic;
using BepInEx.Configuration;

namespace Dusk;

public class MoonSceneConfig(ConfigContext section, string EntityNameReference) : DuskBaseConfig(section, EntityNameReference)
{
    public ConfigEntry<string>? WeatherSpawnWeights;
    public ConfigEntry<int>? BaseWeight;

    override internal List<ConfigEntryBase?> _configEntries => [
        WeatherSpawnWeights,
        BaseWeight
    ];
}