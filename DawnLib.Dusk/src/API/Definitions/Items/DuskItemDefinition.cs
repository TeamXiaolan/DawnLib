using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Dawn;
using Dawn.Utils;
using Dusk.Weights;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dusk;

[CreateAssetMenu(fileName = "New Item Definition", menuName = $"{DuskModConstants.Definitions}/Item Definition")]
public class DuskItemDefinition : DuskContentDefinition<DawnItemInfo>
{
    [field: FormerlySerializedAs("item")]
    [field: SerializeField]
    public Item Item { get; private set; }

    [field: SerializeField]
    public ShopItemPreset ShopItemPreset { get; private set; } = new();

    [field: SerializeField]
    public DuskTerminalPredicate TerminalPredicate { get; private set; }

    [field: SerializeField]
    public DuskPricingStrategy PricingStrategy { get; private set; }

    [field: Space(10)]
    [field: Header("Configs | Spawn Weights")]
    [field: SerializeField]
    public string MoonSpawnWeights { get; private set; }
    [field: SerializeField]
    public string InteriorSpawnWeights { get; private set; }
    [field: SerializeField]
    public string WeatherSpawnWeights { get; private set; }
    [field: SerializeField]
    public bool GenerateSpawnWeightsConfig { get; private set; } = true;

    [field: Header("Configs | Scrap")]
    [field: SerializeField]
    public bool IsScrap { get; private set; }
    [field: SerializeField]
    public bool GenerateScrapConfig { get; private set; }

    [field: Header("Configs | Shop")]
    [field: SerializeField]
    public bool IsShopItem { get; private set; }
    [field: SerializeField]
    public bool GenerateShopItemConfig { get; private set; }
    [field: SerializeField]
    public int Cost { get; private set; }

    [field: Header("Configs | Misc")]
    [field: SerializeField]
    public bool GenerateDisableUnlockConfig { get; private set; }
    [field: SerializeField]
    public bool GenerateDisablePricingStrategyConfig { get; private set; }

    public SpawnWeightsPreset SpawnWeights { get; private set; } = new();
    public ItemConfig Config { get; private set; }

    public override void Register(DuskMod mod)
    {
        base.Register(mod);
        BoundedRange itemWorth = new(Item.minValue * 0.4f, Item.maxValue * 0.4f);
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateItemConfig(section);

        if (Config.Worth != null)
        {
            BoundedRange configValue = Config.Worth.Value;

            if (configValue.Min == -1 || configValue.Max == -1)
            {
                mod.Logger?.LogInfo($"Migrating scrap value of {Item.itemName} from -1,-1.");
                Config.Worth.Value = itemWorth;
            }
            else
            {
                itemWorth = configValue;
            }
        }

        Item.minValue = (int)(itemWorth.Min / 0.4f);
        Item.maxValue = (int)(itemWorth.Max / 0.4f);

        SpawnWeights.SetupSpawnWeightsPreset(Config.MoonSpawnWeights?.Value ?? MoonSpawnWeights, Config.InteriorSpawnWeights?.Value ?? InteriorSpawnWeights, Config.WeatherSpawnWeights?.Value ?? WeatherSpawnWeights);

        DawnLib.DefineItem(TypedKey, Item, builder =>
        {
            if (Config.IsScrapItem?.Value ?? IsScrap)
            {
                builder.DefineScrap(scrapBuilder =>
                {
                    scrapBuilder.SetWeights(weightBuilder => weightBuilder.SetGlobalWeight(SpawnWeights));
                });
            }

            if (Config.IsShopItem?.Value ?? IsShopItem)
            {
                builder.DefineShop(shopItemBuilder =>
                {
                    shopItemBuilder.OverrideRequestNode(ShopItemPreset.OrderRequestNode);
                    shopItemBuilder.OverrideReceiptNode(ShopItemPreset.OrderReceiptNode);
                    shopItemBuilder.OverrideInfoNode(ShopItemPreset.ItemInfoNode);

                    bool disableUnlockRequirements = Config.DisableUnlockRequirements?.Value ?? false;
                    if (!disableUnlockRequirements && TerminalPredicate)
                    {
                        TerminalPredicate.Register(TypedKey);
                        shopItemBuilder.SetPurchasePredicate(TerminalPredicate);
                    }

                    bool disablePricingStrategy = Config.DisablePricingStrategy?.Value ?? false;
                    if (PricingStrategy && !disablePricingStrategy)
                    {
                        PricingStrategy.Register(Key);
                        shopItemBuilder.OverrideCost(PricingStrategy);
                    }
                    else
                    {
                        shopItemBuilder.OverrideCost(Config.Cost?.Value ?? Cost);
                    }
                });
            }

            ApplyTagsTo(builder);
        });
    }

    public ItemConfig CreateItemConfig(ConfigContext context)
    {
        ConfigEntry<bool>? isScrapItem = GenerateScrapConfig ? context.Bind($"{EntityNameReference} | Is Scrap", $"Whether {EntityNameReference} is a scrap item.", IsScrap) : null;
        ConfigEntry<bool>? isShopItem = GenerateShopItemConfig ? context.Bind($"{EntityNameReference} | Is Shop Item", $"Whether {EntityNameReference} is a shop item.", IsShopItem) : null;

        return new ItemConfig
        {
            MoonSpawnWeights = GenerateSpawnWeightsConfig ? context.Bind($"{EntityNameReference} | Preset Moon Weights", $"Preset moon weights for {EntityNameReference}.", MoonSpawnWeights) : null,
            InteriorSpawnWeights = GenerateSpawnWeightsConfig ? context.Bind($"{EntityNameReference} | Preset Interior Weights", $"Preset interior weights for {EntityNameReference}.", InteriorSpawnWeights) : null,
            WeatherSpawnWeights = GenerateSpawnWeightsConfig ? context.Bind($"{EntityNameReference} | Preset Weather Weights", $"Preset weather weights for {EntityNameReference}.", WeatherSpawnWeights) : null,

            DisableUnlockRequirements = GenerateDisableUnlockConfig && TerminalPredicate ? context.Bind($"{EntityNameReference} | Disable Unlock Requirements", $"Whether {EntityNameReference} should have it's unlock requirements disabled.", false) : null,
            DisablePricingStrategy = GenerateDisablePricingStrategyConfig && PricingStrategy ? context.Bind($"{EntityNameReference} | Disable Pricing Strategy", $"Whether {EntityNameReference} should have it's pricing strategy disabled.", false) : null,

            IsScrapItem = isScrapItem,
            Worth = isScrapItem?.Value ?? IsScrap ? context.Bind($"{EntityNameReference} | Value", $"How much {EntityNameReference} is worth when spawning.", new BoundedRange(Item.minValue * 0.4f, Item.maxValue * 0.4f)) : null,

            IsShopItem = isShopItem,
            Cost = isShopItem?.Value ?? IsShopItem ? context.Bind($"{EntityNameReference} | Cost", $"Cost for {EntityNameReference} in the shop.", Cost) : null,
        };
    }

    protected override string EntityNameReference => Item?.itemName ?? string.Empty;
}