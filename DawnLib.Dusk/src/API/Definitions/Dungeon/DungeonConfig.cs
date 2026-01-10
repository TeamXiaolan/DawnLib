using BepInEx.Configuration;
using Dawn.Utils;

namespace Dusk;

public class DungeonConfig
{
    public ConfigEntry<string>? MoonSpawnWeights;
    public ConfigEntry<string>? WeatherSpawnWeights;
    public ConfigEntry<BoundedRange> DungeonRangeClamp;
}