using BepInEx.Configuration;
using Dawn.Utils;

namespace Dusk;

public class DungeonConfig(ConfigContext section, string EntityNameReference) : DuskBaseConfig(section, EntityNameReference)
{
    public ConfigEntry<string>? MoonSpawnWeights;
    public ConfigEntry<string>? WeatherSpawnWeights;
    public ConfigEntry<BoundedRange> DungeonRangeClamp;
}