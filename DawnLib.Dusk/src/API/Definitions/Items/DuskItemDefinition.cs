using System.Collections.Generic;
using Dawn;
using Dawn.Utils;
using Dusk.Weights;
using Unity.Netcode;
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
    public DuskTerminalPredicate? TerminalPredicate { get; private set; }

    [field: SerializeField]
    public DuskPricingStrategy? PricingStrategy { get; private set; }

    [field: Space(10)]
    [field: Header("Configs | Spawn Weights")]
    [field: SerializeField]
    public List<NamespacedConfigWeight> MoonSpawnWeightsConfig { get; private set; } = new();
    [field: SerializeField]
    public List<NamespacedConfigWeight> InteriorSpawnWeightsConfig { get; private set; } = new();
    [field: SerializeField]
    public List<NamespacedConfigWeight> WeatherSpawnWeightsConfig { get; private set; } = new();
    [field: SerializeField]
    public List<IntComparisonConfigWeight> RouteSpawnWeightsConfig { get; private set; } = new();

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
    public bool GenerateDisableUnlockConfig { get; private set; } = true;
    [field: SerializeField]
    public bool GenerateDisablePricingStrategyConfig { get; private set; } = true;

    private IWeightModifierSource<int> _spawnWeightSource = null!;
    public ItemConfig Config { get; private set; }

    public override void Register(DuskMod mod)
    {
        base.Register(mod);
        BoundedRange itemWorth = new(Item.minValue * 0.4f, Item.maxValue * 0.4f);
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        Config = CreateItemConfig(section);
        BaseConfig = Config;

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

        _spawnWeightSource = CreateSpawnWeightSource(
            () => GetConfigWeights(Config.MoonSpawnWeights, MoonSpawnWeightsConfig),
            () => GetConfigWeights(Config.InteriorSpawnWeights, InteriorSpawnWeightsConfig),
            () => GetConfigWeights(Config.WeatherSpawnWeights, WeatherSpawnWeightsConfig),
            () => GetConfigWeights(Config.RouteSpawnWeights, RouteSpawnWeightsConfig),
            () => 0);

        DawnLib.DefineItem(TypedKey, Item, builder =>
        {
            if (Config.IsScrapItem?.Value ?? IsScrap)
            {
                builder.DefineScrap(scrapBuilder =>
                {
                    scrapBuilder.SetWeights(weightProfile => weightProfile.AddSource(_spawnWeightSource));
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
                    if (!disableUnlockRequirements && TerminalPredicate != null)
                    {
                        TerminalPredicate.Register(TypedKey);
                        shopItemBuilder.SetPurchasePredicate(TerminalPredicate);
                    }

                    bool disablePricingStrategy = Config.DisablePricingStrategy?.Value ?? false;
                    if (PricingStrategy != null && !disablePricingStrategy)
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

    public ItemConfig CreateItemConfig(ConfigContext section)
    {
        ItemConfig itemConfig = new(section, EntityNameReference);

        itemConfig.MoonSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Moon Weights", $"Preset moon weights for {EntityNameReference}.", MoonSpawnWeightsConfig.ConvertManyToString()) : null;
        itemConfig.InteriorSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Interior Weights", $"Preset interior weights for {EntityNameReference}.", InteriorSpawnWeightsConfig.ConvertManyToString()) : null;
        itemConfig.WeatherSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Weather Weights", $"Preset weather weights for {EntityNameReference}.", WeatherSpawnWeightsConfig.ConvertManyToString()) : null;
        itemConfig.RouteSpawnWeights = GenerateSpawnWeightsConfig ? section.Bind($"{EntityNameReference} | Preset Route Weights", $"Preset route weights for {EntityNameReference}.", IntComparisonConfigWeight.ConvertManyToString(RouteSpawnWeightsConfig)) : null;

        itemConfig.DisableUnlockRequirements = GenerateDisableUnlockConfig && TerminalPredicate ? section.Bind($"{EntityNameReference} | Disable Unlock Requirements", $"Whether {EntityNameReference} should have it's unlock requirements disabled.", false) : null;
        itemConfig.DisablePricingStrategy = GenerateDisablePricingStrategyConfig && PricingStrategy ? section.Bind($"{EntityNameReference} | Disable Pricing Strategy", $"Whether {EntityNameReference} should have it's pricing strategy disabled.", false) : null;

        itemConfig.IsScrapItem = GenerateScrapConfig ? section.Bind($"{EntityNameReference} | Is Scrap", $"Whether {EntityNameReference} is a scrap item.", IsScrap) : null;
        itemConfig.Worth = itemConfig.IsScrapItem?.Value ?? IsScrap ? section.Bind($"{EntityNameReference} | Value", $"How much {EntityNameReference} is worth when spawning.", new BoundedRange(Item.minValue * 0.4f, Item.maxValue * 0.4f)) : null;

        itemConfig.IsShopItem = GenerateShopItemConfig ? section.Bind($"{EntityNameReference} | Is Shop Item", $"Whether {EntityNameReference} is a shop item.", IsShopItem) : null;
        itemConfig.Cost = itemConfig.IsShopItem?.Value ?? IsShopItem ? section.Bind($"{EntityNameReference} | Cost", $"Cost for {EntityNameReference} in the shop.", Cost) : null;

        if (!itemConfig.UserAllowedToEdit())
        {
            DuskBaseConfig.AssignValueIfNotNull(itemConfig.MoonSpawnWeights, MoonSpawnWeightsConfig.ConvertManyToString());
            DuskBaseConfig.AssignValueIfNotNull(itemConfig.InteriorSpawnWeights, InteriorSpawnWeightsConfig.ConvertManyToString());
            DuskBaseConfig.AssignValueIfNotNull(itemConfig.WeatherSpawnWeights, WeatherSpawnWeightsConfig.ConvertManyToString());
            DuskBaseConfig.AssignValueIfNotNull(itemConfig.RouteSpawnWeights, IntComparisonConfigWeight.ConvertManyToString(RouteSpawnWeightsConfig));

            DuskBaseConfig.AssignValueIfNotNull(itemConfig.DisableUnlockRequirements, false);
            DuskBaseConfig.AssignValueIfNotNull(itemConfig.DisablePricingStrategy, false);

            DuskBaseConfig.AssignValueIfNotNull(itemConfig.IsScrapItem, IsScrap);
            DuskBaseConfig.AssignValueIfNotNull(itemConfig.Worth, new BoundedRange(Item.minValue * 0.4f, Item.maxValue * 0.4f));

            DuskBaseConfig.AssignValueIfNotNull(itemConfig.IsShopItem, IsShopItem);
            DuskBaseConfig.AssignValueIfNotNull(itemConfig.Cost, Cost);
        }
        return itemConfig;
    }


    public override void TryNetworkRegisterAssets()
    {
        if (!Item.spawnPrefab.TryGetComponent(out NetworkObject _))
            return;

        DawnLib.RegisterNetworkPrefab(Item.spawnPrefab);
    }

    protected override string EntityNameReference => Item?.itemName ?? string.Empty;
}