using BepInEx.Configuration;

namespace Dusk;

public class MapObjectConfig(ConfigContext section, string EntityNameReference) : DuskBaseConfig(section, EntityNameReference)
{
    public ConfigEntry<string>? InsideMoonCurveSpawnWeights;
    public ConfigEntry<string>? InsideInteriorCurveSpawnWeights;
    public ConfigEntry<bool>? InsideHazard;
    public ConfigEntry<string>? OutsideMoonCurveSpawnWeights;
    public ConfigEntry<string>? OutsideInteriorCurveSpawnWeights;
    public ConfigEntry<bool>? OutsideHazard;
    public ConfigEntry<bool>? InsidePrioritiseMoon;
    public ConfigEntry<bool>? OutsidePrioritiseMoon;
}