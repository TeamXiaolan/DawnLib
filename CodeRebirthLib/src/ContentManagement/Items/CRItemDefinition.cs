using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using CodeRebirthLib.AssetManagement;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.Data;
using LethalLib.Modules;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeRebirthLib.ContentManagement.Items;

[CreateAssetMenu(fileName = "New Item Definition", menuName = "CodeRebirthLib/Item Definition")]
public class CRItemDefinition : CRContentDefinition<ItemData>
{
    [field: FormerlySerializedAs("item"), SerializeField]
    public Item Item { get; private set; }
    
    [field: FormerlySerializedAs("terminalNode"), SerializeField]
    public TerminalNode? TerminalNode { get; private set; }

    public ItemConfig Config { get; private set; }
    
    public override void Register(CRMod mod, ItemData data)
    {
        Config = CreateItemConfig(mod, data, Item.itemName);

        BoundedRange itemWorth = Config.Worth.Value ?? new BoundedRange(-1, -1);
        if (itemWorth.Min != -1 && itemWorth.Max != -1)
        {
            Item.minValue = (int)(itemWorth.Min / 0.4f);
            Item.maxValue = (int)(itemWorth.Max / 0.4f);
        }

        if (Config.IsShopItem.Value)
        {
            LethalLib.Modules.Items.RegisterShopItem(Item, null, null, TerminalNode, Config.Cost.Value);
        }

        if (Config.IsScrapItem.Value)
        {
            (Dictionary<Levels.LevelTypes, int> spawnRateByLevelType, Dictionary<string, int> spawnRateByCustomLevelType) = ConfigManager.ParseMoonsWithRarity(Config.SpawnWeights.Value);
            LethalLib.Modules.Items.RegisterScrap(Item, spawnRateByLevelType, spawnRateByCustomLevelType);
        }
    }
    
    public static ItemConfig CreateItemConfig(CRMod mod, ItemData data, string itemName)
    {
        using(ConfigContext section = mod.ConfigManager.CreateConfigSection(itemName))
        {
            ConfigEntry<bool> isScrapItem = section.Bind("Is Scrap", $"Whether {itemName} is a scrap item.", data.isScrap);
            ConfigEntry<bool> isShopItem = section.Bind("Is Shop Item", $"Whether {itemName} is a shop item..", data.isShopItem);
            
            return new ItemConfig
            {
                SpawnWeights = section.Bind("Spawn Weights", $"Spawn weights for {itemName}.", data.spawnWeights),
                IsScrapItem = isScrapItem,
                Worth = isScrapItem.Value? section.Bind("Value", $"How much {itemName} is worth when spawning. -1,-1 is the default.", new BoundedRange(-1, -1)) : null,
                IsShopItem = isShopItem,
                Cost = isShopItem.Value? section.Bind("Cost", $"Cost for {itemName} in the shop.", data.cost) : null
            };
        }
    }
    
    public override List<ItemData> GetEntities(CRMod mod) => mod.Content.assetBundles.SelectMany(it => it.items).ToList();
}