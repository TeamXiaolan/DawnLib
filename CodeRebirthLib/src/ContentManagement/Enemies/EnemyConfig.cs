using BepInEx.Configuration;

namespace CodeRebirthLib.ContentManagement.Enemies;
public class EnemyConfig
{
    public ConfigEntry<int> MaxSpawnCount;
    public ConfigEntry<float> PowerLevel;
    public ConfigEntry<string> SpawnWeights;
    public ConfigEntry<string> WeatherMultipliers;
}