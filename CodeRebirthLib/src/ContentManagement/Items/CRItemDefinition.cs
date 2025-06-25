using System.Collections.Generic;
using System.Linq;
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
        Config = CreateItemConfig(section, data, Item.itemName);

        BoundedRange itemWorth = Config.Worth?.Value ?? new BoundedRange(-1, -1);
        if (itemWorth.Min != -1 && itemWorth.Max != -1)
        {
            Item.minValue = (int)(itemWorth.Min / 0.4f);
            Item.maxValue = (int)(itemWorth.Max / 0.4f);
        }

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

    public static ItemConfig CreateItemConfig(ConfigContext section, ItemData data, string itemName)
    {
        var isScrapItem = data.generateScrapConfig ? section.Bind("Is Scrap", $"Whether {itemName} is a scrap item.", data.isScrap) : null;
        var isShopItem = data.generateShopItemConfig ? section.Bind("Is Shop Item", $"Whether {itemName} is a shop item.", data.isShopItem) : null;

        return new ItemConfig
        {
            SpawnWeights = data.generateSpawnWeightsConfig ? section.Bind("Spawn Weights", $"Spawn weights for {itemName}.", data.spawnWeights) : null,
            IsScrapItem = isScrapItem,
            Worth = isScrapItem?.Value ?? false ? section.Bind("Value", $"How much {itemName} is worth when spawning. -1,-1 is the default.", new BoundedRange(-1, -1)) : null,
            IsShopItem = isShopItem,
            Cost = isShopItem?.Value ?? false ? section.Bind("Cost", $"Cost for {itemName} in the shop.", data.cost) : null,
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