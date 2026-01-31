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
    public ConfigEntry<AnimationCurve>? InsideEnemySpawnCurve = null;
    public ConfigEntry<AnimationCurve>? OutsideEnemySpawnCurve = null;
    public ConfigEntry<AnimationCurve>? DaytimeEnemySpawnCurve = null;
    public ConfigEntry<float>? InsideEnemySpawnRange = null;
    public ConfigEntry<float>? OutsideEnemySpawnRange = null;
    public ConfigEntry<float>? DaytimeEnemySpawnRange = null;
}