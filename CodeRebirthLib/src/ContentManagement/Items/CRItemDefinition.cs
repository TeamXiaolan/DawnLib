using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.ConfigManagement.Weights;
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

    [field: SerializeField]
    public SpawnWeightsPreset SpawnWeights { get; private set; }

    [field: FormerlySerializedAs("terminalNode")] [field: SerializeField]
    public TerminalNode? TerminalNode { get; private set; }

    public ItemConfig Config { get; private set; }

    protected override string EntityNameReference => Item.itemName;

    public override void Register(CRMod mod, ItemData data)
    {
        if (SpawnWeights == null)
        {
            SpawnWeights = ScriptableObject.CreateInstance<SpawnWeightsPreset>();
        }

        BoundedRange itemWorth = new BoundedRange(Item.minValue * 0.4f, Item.maxValue * 0.4f);
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

        if (Config.IsShopItem?.Value ?? data.isShopItem)
        {
            // TODO Register our own shop item
            LethalLib.Modules.Items.RegisterShopItem(Item, null, null, TerminalNode, Config.Cost?.Value ?? data.cost);
        }

        if (Config.MoonSpawnWeights != null && Config.InteriorSpawnWeights != null && Config.WeatherSpawnWeights != null)
        {
            SpawnWeights.SetupSpawnWeightsPreset(Config.MoonSpawnWeights.Value, Config.InteriorSpawnWeights.Value, Config.WeatherSpawnWeights.Value);
        }

        CRLib.RegisterScrap(Item, "All", SpawnWeights);
        mod.ItemRegistry().Register(this);
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

    internal static void UpdateAllWeights(SelectableLevel? level = null)
    {
        if (!StartOfRound.Instance)
            return;

        SelectableLevel levelToUpdate = level ?? StartOfRound.Instance.currentLevel;

        foreach (var spawnableItemWithRarity in levelToUpdate.spawnableScrap)
        {
            if (!spawnableItemWithRarity.spawnableItem.TryGetDefinition(out CRItemDefinition? definition))
                continue;

            spawnableItemWithRarity.rarity = definition.SpawnWeights.GetWeight();
        }
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