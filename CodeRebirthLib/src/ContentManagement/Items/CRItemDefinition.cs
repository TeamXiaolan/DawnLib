using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using CodeRebirthLib.AssetManagement;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.Data;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeRebirthLib.ContentManagement.Items;
[CreateAssetMenu(fileName = "New Item Definition", menuName = "CodeRebirthLib/Definitions/Item Definition")]
public class CRItemDefinition : CRContentDefinition<ItemData>
{
    public const string REGISTRY_ID = "items";

    [field: FormerlySerializedAs("item")] [field: SerializeField]
    public Item Item { get; private set; }

    [field: FormerlySerializedAs("terminalNode")] [field: SerializeField]
    public TerminalNode? TerminalNode { get; private set; }

    public ItemConfig Config { get; private set; }

    public override void Register(CRMod mod, ItemData data)
    {
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        BoundedRange itemWorth = new BoundedRange(Item.minValue * .4f, Item.maxValue * .4f);
        Config = CreateItemConfig(section, data, itemWorth, Item.itemName);

        if (Config.Worth != null)
        {
            BoundedRange configValue = Config.Worth.Value;
            
            // Perform migration:
            if (configValue.Min == -1 || configValue.Max == -1)
            {
                mod.Logger?.LogInfo($"Migrating scrap value of {Item.itemName} from -1,-1.");
                Config.Worth.Value = itemWorth; // itemWorth hasn't been updated here, so by setting a new value, it effectively changes from -1,-1 to the default item worth from above.
            }
            else
            {
                itemWorth = configValue;
            }
        }
        
        Item.minValue = (int)(itemWorth.Min / 0.4f);
        Item.maxValue = (int)(itemWorth.Max / 0.4f);

        if (Config.IsShopItem?.Value ?? data.isShopItem)
        {
            LethalLib.Modules.Items.RegisterShopItem(Item, null, null, TerminalNode, Config.Cost?.Value ?? data.cost);
        }

        if (Config.IsScrapItem?.Value ?? data.isScrap)
        {
            (var spawnRateByLevelType, Dictionary<string, int> spawnRateByCustomLevelType) = ConfigManager.ParseMoonsWithRarity(Config.SpawnWeights?.Value ?? data.spawnWeights);
            LethalLib.Modules.Items.RegisterScrap(Item, spawnRateByLevelType, spawnRateByCustomLevelType);
        }

        mod.ItemRegistry().Register(this);
    }

    public static ItemConfig CreateItemConfig(ConfigContext section, ItemData data, BoundedRange defaultScrapValue, string itemName)
    {
        ConfigEntry<bool>? isScrapItem = data.generateScrapConfig ? section.Bind($"{itemName} | Is Scrap", $"Whether {itemName} is a scrap item.", data.isScrap) : null;
        ConfigEntry<bool>? isShopItem = data.generateShopItemConfig ? section.Bind($"{itemName} | Is Shop Item", $"Whether {itemName} is a shop item.", data.isShopItem) : null;

        return new ItemConfig
        {
            SpawnWeights = data.generateSpawnWeightsConfig ? section.Bind($"{itemName} | Spawn Weights", $"Spawn weights for {itemName}.", data.spawnWeights) : null,
            IsScrapItem = isScrapItem,
            Worth = isScrapItem?.Value ?? data.isScrap ? section.Bind($"{itemName} | Value", $"How much {itemName} is worth when spawning.", defaultScrapValue) : null,
            IsShopItem = isShopItem,
            Cost = isShopItem?.Value ?? data.isShopItem ? section.Bind($"{itemName} | Cost", $"Cost for {itemName} in the shop.", data.cost) : null,
        };
    }

    public static void RegisterTo(CRMod mod)
    {
        mod.CreateRegistry(REGISTRY_ID, new CRRegistry<CRItemDefinition>());
    }

    public override List<ItemData> GetEntities(CRMod mod)
    {
        return mod.Content.assetBundles.SelectMany(it => it.items).ToList();
    }
}