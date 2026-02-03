using BepInEx.Configuration;

namespace Dusk;

public class EnemyConfig(ConfigContext section, string EntityNameReference) : DuskBaseConfig(section, EntityNameReference)
{
    public ConfigEntry<string>? MoonSpawnWeights;
    public ConfigEntry<string>? InteriorSpawnWeights;
    public ConfigEntry<string>? WeatherSpawnWeights;
    public ConfigEntry<float>? PowerLevel;
    public ConfigEntry<int>? MaxSpawnCount;
}