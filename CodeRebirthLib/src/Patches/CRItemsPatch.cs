using System;
using System.Collections.Generic;
using System.Linq;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.ContentManagement.Items;
using IL.LethalQuantities;
using UnityEngine;

namespace CodeRebirthLib.Patches;

static class CRItemsPatch
{
    private static List<CRItemDefinition> registeredCRItems => CRMod.AllItems().ToList();

    internal static void Init()
    {
        On.StartOfRound.Awake += StartOfRound_Awake;
        On.RoundManager.SpawnScrapInLevel += RoundManager_SpawnScrapInLevel;
    }

    private static void StartOfRound_Awake(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        orig(self);
        foreach (var CRItemDefinition in registeredCRItems)
        {
            self.allItemsList.itemsList.Add(CRItemDefinition.Item);
            foreach (var level in self.levels)
            {
                var spawnableItemWithRarity = new SpawnableItemWithRarity()
                {
                    spawnableItem = CRItemDefinition.Item,
                    rarity = 0 // TODO !! get the base weight and put it here by default? update weights on interior change and weather change.
                };
                level.spawnableScrap.Add(spawnableItemWithRarity);
            }
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