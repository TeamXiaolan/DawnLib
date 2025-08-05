using BepInEx.Configuration;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.Data;

namespace CodeRebirthLib.ContentManagement.Items;
public class ItemConfig : CRContentConfig
{
    public ConfigEntry<int>? Cost;
    public ConfigEntry<bool>? IsScrapItem;
    public ConfigEntry<bool>? IsShopItem;
    public ConfigEntry<int>? PresetsBaseWeight;
    public ConfigEntry<string>? PresetsSpawnWeights;
    public ConfigEntry<BoundedRange>? Worth;
}