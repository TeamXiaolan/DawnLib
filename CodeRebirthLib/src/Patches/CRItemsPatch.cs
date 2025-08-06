using System.Collections.Generic;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.ConfigManagement.Weights;
using CodeRebirthLib.ContentManagement.Items;
using UnityEngine;

namespace CodeRebirthLib.Patches;

static class CRItemsPatch
{
    static readonly Dictionary<SpawnWeightsPreset, List<Item>> itemsToInjectThroughPreset = [];

    internal static void Init()
    {
        On.StartOfRound.Awake += StartOfRound_Awake;
        On.RoundManager.SpawnScrapInLevel += RoundManager_SpawnScrapInLevel;
    }

    internal static void AddItemForLevel(SpawnWeightsPreset spawnWeightsPreset, Item item)
    {
        if (!itemsToInjectThroughPreset.TryGetValue(spawnWeightsPreset, out List<Item> items))
        {
            items = new();
        }
        items.Add(item);
        itemsToInjectThroughPreset[spawnWeightsPreset] = items;
    }

    private static void StartOfRound_Awake(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        orig(self);
        foreach (var spawnPresetsWithItemsIn in itemsToInjectThroughPreset)
        {
            /*var level = ConfigManager.GetLevelWithName(spawnPresetsWithItemsIn.Key);
            if (level == null)
                continue;

            foreach (Item item in spawnPresetsWithItemsIn.Value)
            {
                if (!item.TryGetDefinition(out CRItemDefinition? CRItemDefinition))
                    continue;

                if (!self.allItemsList.itemsList.Contains(CRItemDefinition.Item))
                {
                    self.allItemsList.itemsList.Add(CRItemDefinition.Item);
                }

                var spawnableItemWithRarity = new SpawnableItemWithRarity()
                {
                    spawnableItem = CRItemDefinition.Item,
                    rarity = 0 // TODO !! get the base weight and put it here by default? update weights on interior change and weather change.
                };
                level.spawnableScrap.Add(spawnableItemWithRarity);
            }*/
        }
    }

    private static void RoundManager_SpawnScrapInLevel(On.RoundManager.orig_SpawnScrapInLevel orig, RoundManager self)
    {
        foreach (var scrapWithRarity in self.currentLevel.spawnableScrap)
        {
            if (scrapWithRarity.spawnableItem.TryGetDefinition(out CRItemDefinition? definition)) // todo pls make this better so we dont loop through a million billion lists on top of a list
            {
                CodeRebirthLibPlugin.ExtendedLogging($"setting up {scrapWithRarity.spawnableItem.itemName} to be spawnable in this level.");
                // change the weight based on the interior, weather adn moon
                int newWeight = Mathf.FloorToInt(definition.SpawnWeights.GetWeight());
                scrapWithRarity.rarity = newWeight;
            }
        }
        orig(self);
    }
}