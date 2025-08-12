using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using CodeRebirthLib.CRMod.Weights;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeRebirthLib.CRMod;

[CreateAssetMenu(fileName = "New Item Definition", menuName = "CodeRebirthLib/Definitions/Item Definition")]
public class CRItemDefinition : CRContentDefinition<ItemData, CRItemInfo>
{
    public const string REGISTRY_ID = "items";

    [field: FormerlySerializedAs("item")]
    [field: SerializeField]
    public Item Item { get; private set; }

    [field: SerializeField]
    public ShopItemPreset ShopItemPreset { get; private set; } = new();

    public SpawnWeightsPreset SpawnWeights { get; private set; } = new();
    public ItemConfig Config { get; private set; }

    protected override string EntityNameReference => Item.itemName;

    public override void Register(CRMod mod, ItemData data)
    {
        BoundedRange itemWorth = new(Item.minValue * 0.4f, Item.maxValue * 0.4f);
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateItemConfig(section, data, itemWorth, SpawnWeights, Item.itemName);

        if (Config.Worth != null)
        {
            BoundedRange configValue = Config.Worth.Value;

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

        SpawnWeights.SetupSpawnWeightsPreset(Config.MoonSpawnWeights?.Value ?? data.moonSpawnWeights, Config.InteriorSpawnWeights?.Value ?? data.interiorSpawnWeights, Config.WeatherSpawnWeights?.Value ?? data.weatherSpawnWeights);

        CRLib.DefineItem(null, Item, builder =>
        {
            if (Config.IsScrapItem?.Value ?? data.isScrap)
            {
                builder.DefineScrap(scrapBuilder =>
                {
                    scrapBuilder.SetWeights(weightBuilder => weightBuilder.SetGlobalWeight(SpawnWeights));
                });
            }

            if (Config.IsShopItem?.Value ?? data.isShopItem)
            {
                builder.DefineShop(shopItemBuilder =>
                {
                    shopItemBuilder.OverrideRequestNode(ShopItemPreset.OrderRequestNode);
                    shopItemBuilder.OverrideReceiptNode(ShopItemPreset.OrderReceiptNode);
                    shopItemBuilder.OverrideInfoNode(ShopItemPreset.ItemInfoNode);
                    shopItemBuilder.OverrideCost(Config.Cost?.Value ?? data.cost);
                });
            }
        });
    }

    public static ItemConfig CreateItemConfig(ConfigContext section, ItemData data, BoundedRange defaultScrapValue, SpawnWeightsPreset spawnWeightsPreset, string itemName)
    {
        ConfigEntry<bool>? isScrapItem = data.generateScrapConfig ? section.Bind($"{itemName} | Is Scrap", $"Whether {itemName} is a scrap item.", data.isScrap) : null;
        ConfigEntry<bool>? isShopItem = data.generateShopItemConfig ? section.Bind($"{itemName} | Is Shop Item", $"Whether {itemName} is a shop item.", data.isShopItem) : null;

        return new ItemConfig
        {
            MoonSpawnWeights = data.generateSpawnWeightsConfig ? section.Bind($"{itemName} | Preset Moon Weights", $"Preset moon weights for {itemName}.", spawnWeightsPreset.MoonSpawnWeightsTransformer.ToConfigString()) : null,
            InteriorSpawnWeights = data.generateSpawnWeightsConfig ? section.Bind($"{itemName} | Preset Interior Weights", $"Preset interior weights for {itemName}.", spawnWeightsPreset.InteriorSpawnWeightsTransformer.ToConfigString()) : null,
            WeatherSpawnWeights = data.generateSpawnWeightsConfig ? section.Bind($"{itemName} | Preset Weather Weights", $"Preset weather weights for {itemName}.", spawnWeightsPreset.WeatherSpawnWeightsTransformer.ToConfigString()) : null,
            IsScrapItem = isScrapItem,
            Worth = isScrapItem?.Value ?? data.isScrap ? section.Bind($"{itemName} | Value", $"How much {itemName} is worth when spawning.", defaultScrapValue) : null,
            IsShopItem = isShopItem,
            Cost = isShopItem?.Value ?? data.isShopItem ? section.Bind($"{itemName} | Cost", $"Cost for {itemName} in the shop.", data.cost) : null,
        };
    }

    public override List<ItemData> GetEntities(CRMod mod)
    {
        return mod.Content.assetBundles.SelectMany(it => it.items).ToList();
    }
}