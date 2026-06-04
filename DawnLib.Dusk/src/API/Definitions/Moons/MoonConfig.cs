using System.Collections.Generic;
using BepInEx.Configuration;
using Dawn.Utils;
using UnityEngine;

namespace Dusk;

public class MoonConfig(ConfigContext section, string EntityNameReference) : DuskBaseConfig(section, EntityNameReference)
{
    public ConfigEntry<int>? Cost = null;

    public ConfigEntry<float>? TimeFactor = null;

    public ConfigEntry<bool>? DisableUnlockRequirements = null;
    public ConfigEntry<bool>? DisablePricingStrategy = null;

    public ConfigEntry<BoundedRange>? MinMaxScrap = null;

    public ConfigEntry<int>? InsideEnemyPowerCount = null;
    public ConfigEntry<int>? OutsideEnemyPowerCount = null;
    public ConfigEntry<int>? DaytimeEnemyPowerCount = null;
    public ConfigEntry<int>? WeedEnemyPowerCount = null;

    public ConfigEntry<int>? InsideDiversityPowerCount = null;
    public ConfigEntry<int>? OutsideDiversityPowerCount = null;
    public ConfigEntry<int>? DaytimeDiversityPowerCount = null;
    public ConfigEntry<int>? WeedDiversityPowerCount = null;

    public ConfigEntry<AnimationCurve>? InsideEnemySpawnCurve = null;
    public ConfigEntry<AnimationCurve>? OutsideEnemySpawnCurve = null;
    public ConfigEntry<AnimationCurve>? DaytimeEnemySpawnCurve = null;
    public ConfigEntry<AnimationCurve>? WeedEnemySpawnCurve = null;

    public ConfigEntry<float>? InsideEnemySpawnRange = null;
    public ConfigEntry<float>? OutsideEnemySpawnRange = null;
    public ConfigEntry<float>? DaytimeEnemySpawnRange = null;
    public ConfigEntry<float>? WeedEnemySpawnRange = null;

    override internal List<ConfigEntryBase?> _configEntries => [
        Cost,

        TimeFactor,

        DisableUnlockRequirements,
        DisablePricingStrategy,

        MinMaxScrap,

        InsideEnemyPowerCount,
        OutsideEnemyPowerCount,
        DaytimeEnemyPowerCount,
        WeedEnemyPowerCount,

        InsideEnemySpawnCurve,
        OutsideEnemySpawnCurve,
        DaytimeEnemySpawnCurve,
        WeedEnemySpawnCurve,

        InsideEnemySpawnRange,
        OutsideEnemySpawnRange,
        DaytimeEnemySpawnRange,
        WeedEnemySpawnRange
    ];
}