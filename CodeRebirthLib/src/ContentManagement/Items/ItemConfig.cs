using BepInEx.Configuration;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.Data;

namespace CodeRebirthLib.ContentManagement.Items;
public class ItemConfig : CRContentConfig
{
    public ConfigEntry<string>? SpawnWeights;
    public ConfigEntry<bool>? IsScrapItem;
    public ConfigEntry<BoundedRange>? Worth;
    public ConfigEntry<bool>? IsShopItem;
    public ConfigEntry<int>? Cost;
}