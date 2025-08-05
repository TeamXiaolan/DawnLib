using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.ConfigManagement.Weights;
using CodeRebirthLib.Data;
using CodeRebirthLib.Patches;
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

    // put SpawnWeightsPreset somewhere near here?
    protected override string EntityNameReference => Item.itemName;

    public override void Register(CRMod mod, ItemData data)
    {
        using ConfigContext section = mod.ConfigManager.CreateConfigSectionForBundleData(AssetBundleData);
        BoundedRange itemWorth = new BoundedRange(Item.minValue * 0.4f, Item.maxValue * 0.4f);
        if (SpawnWeights == null)
        {
            SpawnWeights = ScriptableObject.CreateInstance<SpawnWeightsPreset>();
        }
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
            // Register our own shop item
            StartOfRoundPatch.registeredCRItems.Add(this);
            LethalLib.Modules.Items.RegisterShopItem(Item, null, null, TerminalNode, Config.Cost?.Value ?? data.cost);
        }

        if (Config.PresetsBaseWeight != null && Config.PresetsSpawnWeights != null)
        {
            // they both wouldnt be null together and atleast presets base weight has to be 0
            SpawnWeights.SetupSpawnWeightsPreset(Config.PresetsBaseWeight.Value, Config.PresetsSpawnWeights.Value);
        }

        mod.ItemRegistry().Register(this);
    }

    public static ItemConfig CreateItemConfig(ConfigContext section, ItemData data, BoundedRange defaultScrapValue, SpawnWeightsPreset spawnWeightsPreset, string itemName)
    {
        ConfigEntry<bool>? isScrapItem = data.generateScrapConfig ? section.Bind($"{itemName} | Is Scrap", $"Whether {itemName} is a scrap item.", data.isScrap) : null;
        ConfigEntry<bool>? isShopItem = data.generateShopItemConfig ? section.Bind($"{itemName} | Is Shop Item", $"Whether {itemName} is a shop item.", data.isShopItem) : null;

        return new ItemConfig
        {
            PresetsBaseWeight = data.generateSpawnWeightsConfig ? section.Bind($"{itemName} | Preset Base Weight", $"Base weight of {itemName} before any transformative operations are applied.", spawnWeightsPreset.BaseWeight) : null,
            PresetsSpawnWeights = data.generateSpawnWeightsConfig ? GenerateSpawnWeightsConfig(section, itemName, spawnWeightsPreset) : null,
            IsScrapItem = isScrapItem,
            Worth = isScrapItem?.Value ?? data.isScrap ? section.Bind($"{itemName} | Value", $"How much {itemName} is worth when spawning.", defaultScrapValue) : null,
            IsShopItem = isShopItem,
            Cost = isShopItem?.Value ?? data.isShopItem ? section.Bind($"{itemName} | Cost", $"Cost for {itemName} in the shop.", data.cost) : null,
        };
    }

    public static ConfigEntry<string> GenerateSpawnWeightsConfig(ConfigContext section, string itemName, SpawnWeightsPreset preset)
    {
        string configString = string.Empty;
        if (preset == null)
        {
            return section.Bind($"{itemName} | Preset Spawn Weights", $"Spawn weights for {itemName}.", configString);
        }

        foreach (var weightTransformer in preset.SpawnWeightsTransformers)
        {
            configString += weightTransformer.ToConfigString();
        }

        return section.Bind($"{itemName} | Preset Spawn Weights", $"Spawn weights for {itemName}.", configString);
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