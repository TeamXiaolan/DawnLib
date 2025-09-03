using BepInEx.Configuration;

namespace CodeRebirthLib.CRMod;

public class MapObjectConfig : EntityConfig
{
    public ConfigEntry<string>? InsideCurveSpawnWeights;
    public ConfigEntry<bool>? InsideHazard;
    public ConfigEntry<string>? OutsideCurveSpawnWeights;
    public ConfigEntry<bool>? OutsideHazard;
}