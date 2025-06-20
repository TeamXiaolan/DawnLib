using BepInEx.Configuration;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.Data;

namespace CodeRebirthLib.ContentManagement.Items;
public class ItemConfig : CRContentConfig
{
    public ConfigEntry<int>? Cost;
    public ConfigEntry<bool>? IsScrapItem;
    public ConfigEntry<bool>? IsShopItem;
    public ConfigEntry<string>? SpawnWeights;
    public ConfigEntry<BoundedRange>? Worth;
}