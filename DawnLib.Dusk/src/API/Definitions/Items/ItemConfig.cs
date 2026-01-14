using BepInEx.Configuration;
using Dawn.Utils;

namespace Dusk;
public class ItemConfig
{
    public ConfigEntry<int>? Cost;
    public ConfigEntry<bool>? IsScrapItem;
    public ConfigEntry<bool>? IsShopItem;
    public ConfigEntry<bool>? DisableUnlockRequirements = null;
    public ConfigEntry<bool>? DisablePricingStrategy = null;
    public ConfigEntry<string>? MoonSpawnWeights;
    public ConfigEntry<string>? InteriorSpawnWeights;
    public ConfigEntry<string>? WeatherSpawnWeights;
    public ConfigEntry<BoundedRange>? Worth;
}