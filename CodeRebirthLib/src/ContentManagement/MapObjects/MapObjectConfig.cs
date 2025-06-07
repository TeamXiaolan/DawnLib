using BepInEx.Configuration;
using CodeRebirthLib.ConfigManagement;

namespace CodeRebirthLib.ContentManagement.MapObjects;
public class MapObjectConfig : CRContentConfig
{
    public ConfigEntry<string>? InsideCurveSpawnWeights;
    public ConfigEntry<bool> InsideHazard;
    public ConfigEntry<string>? OutsideCurveSpawnWeights;
    public ConfigEntry<bool> OutsideHazard;
}