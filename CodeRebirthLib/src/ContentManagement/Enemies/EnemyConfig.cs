using BepInEx.Configuration;

namespace CodeRebirthLib.ContentManagement.Enemies;
public class EnemyConfig
{
    public ConfigEntry<string> SpawnWeights;
    public ConfigEntry<string> WeatherMultipliers;
    public ConfigEntry<float> PowerLevel;
    public ConfigEntry<int> MaxSpawnCount;
}