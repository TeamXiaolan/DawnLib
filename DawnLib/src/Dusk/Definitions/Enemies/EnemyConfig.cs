using BepInEx.Configuration;

namespace Dawn.Dusk;

public class EnemyConfig : EntityConfig
{
    public ConfigEntry<int> MaxSpawnCount;
    public ConfigEntry<float> PowerLevel;
    public ConfigEntry<string>? MoonSpawnWeights;
    public ConfigEntry<string>? InteriorSpawnWeights;
    public ConfigEntry<string>? WeatherSpawnWeights;
}