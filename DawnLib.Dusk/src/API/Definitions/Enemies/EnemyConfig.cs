using BepInEx.Configuration;

namespace Dusk;

public class EnemyConfig
{
    public ConfigEntry<int> MaxSpawnCount;
    public ConfigEntry<float> PowerLevel;
    public ConfigEntry<string>? MoonSpawnWeights;
    public ConfigEntry<string>? InteriorSpawnWeights;
    public ConfigEntry<string>? WeatherSpawnWeights;
}