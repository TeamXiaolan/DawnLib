using BepInEx.Configuration;

namespace Dusk;

public class MapObjectConfig
{
    public ConfigEntry<string>? InsideCurveSpawnWeights;
    public ConfigEntry<bool>? InsideHazard;
    public ConfigEntry<string>? OutsideCurveSpawnWeights;
    public ConfigEntry<bool>? OutsideHazard;
}