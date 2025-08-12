using BepInEx.Configuration;

namespace CodeRebirthLib.CRMod;
public class ItemConfig : EntityConfig
{
    public ConfigEntry<int>? Cost;
    public ConfigEntry<bool>? IsScrapItem;
    public ConfigEntry<bool>? IsShopItem;
    public ConfigEntry<bool>? IsProgressive = null;
    public ConfigEntry<string>? MoonSpawnWeights;
    public ConfigEntry<string>? InteriorSpawnWeights;
    public ConfigEntry<string>? WeatherSpawnWeights;
    public ConfigEntry<BoundedRange>? Worth;
}