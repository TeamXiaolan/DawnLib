using System.Collections.Generic;
using BepInEx.Configuration;

namespace Dusk;

public class WeatherConfig(ConfigContext section, string EntityNameReference) : DuskBaseConfig(section, EntityNameReference)
{
    public ConfigEntry<string>? MoonSpawnWeights;
    public ConfigEntry<string>? WeatherToWeatherSpawnWeights;
    public ConfigEntry<string>? RouteSpawnWeights;

    public ConfigEntry<float> ScrapValueMultiplier;
    public ConfigEntry<float> ScrapAmountMultiplier;

    override internal List<ConfigEntryBase?> _configEntries => [
        MoonSpawnWeights,
        WeatherToWeatherSpawnWeights,
        RouteSpawnWeights,

        ScrapValueMultiplier,
        ScrapAmountMultiplier
    ];
}