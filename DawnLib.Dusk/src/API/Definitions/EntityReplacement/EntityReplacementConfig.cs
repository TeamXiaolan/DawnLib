using BepInEx.Configuration;

namespace Dusk;

public class EntityReplacementConfig
{
    public ConfigEntry<string>? MoonSpawnWeights;
    public ConfigEntry<string>? InteriorSpawnWeights;
    public ConfigEntry<string>? WeatherSpawnWeights;
    public ConfigEntry<bool>? DisableDateCheck;
}