using System.Collections.Generic;
using System.Linq;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.ConfigManagement.Weights;
using CodeRebirthLib.ContentManagement.Items;
using UnityEngine;

namespace CodeRebirthLib.Patches;

static class CRItemsPatch
{
    internal class ItemInjectionSettings(Item item, IWeightProvider rarityProvider)
    {
        public Item Item { get; } = item;
        public IWeightProvider RarityProvider { get; } = rarityProvider;
    }
    
    static readonly Dictionary<string, List<ItemInjectionSettings>> _itemsToInject = [];
    private static readonly Dictionary<SpawnableItemWithRarity, ItemInjectionSettings> _itemSettingsMap = [];

    internal static void Init()
    {
        On.StartOfRound.Awake += StartOfRound_Awake;
        On.RoundManager.SpawnScrapInLevel += RoundManager_SpawnScrapInLevel;
    }

    internal static void AddItemForLevel(string levelName, ItemInjectionSettings settings)
    {
        if (!_itemsToInject.TryGetValue(levelName, out List<ItemInjectionSettings> items))
        {
            items = new();
        }
        items.Add(settings);
        _itemsToInject[levelName] = items;
    }

    private static void StartOfRound_Awake(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        orig(self);
        foreach (SelectableLevel level in StartOfRound.Instance.levels)
        {
            List<ItemInjectionSettings> items = [];
            
            // todo: should this actually be "All" instead of "*"? All i think is better for configs, but by having * here, it could mean new mods using
            // just the CRLib public methods start using *:30 instead of All:30?
            if(_itemsToInject.TryGetValue("*", out List<ItemInjectionSettings> globalItems))
                items.AddRange(globalItems);
            
            // todo: is this where the proper GetLLLMoonName should be used?
            if(_itemsToInject.TryGetValue(level.name, out List<ItemInjectionSettings> moonSpecificItems))
                items.AddRange(moonSpecificItems);

            foreach (ItemInjectionSettings item in items)
            {
                SpawnableItemWithRarity spawnDef = new SpawnableItemWithRarity
                {
                    spawnableItem = item.Item,
                    rarity = item.RarityProvider.GetWeight() // get an inital weight, incase a mod doesn't use any special code.
                };
                level.spawnableScrap.Add(spawnDef);
                _itemSettingsMap[spawnDef] = item;
            }
        }

        foreach (ItemInjectionSettings item in _itemsToInject.Values.SelectMany(it => it))
        {
            self.allItemsList.itemsList.Add(item.Item);
        }
        
        _itemsToInject.Clear();
    }

    private static void RoundManager_SpawnScrapInLevel(On.RoundManager.orig_SpawnScrapInLevel orig, RoundManager self)
    {
        foreach (SpawnableItemWithRarity scrapWithRarity in self.currentLevel.spawnableScrap)
        {
            if(!_itemSettingsMap.TryGetValue(scrapWithRarity, out ItemInjectionSettings settings))
                continue;

            // update weights just before spawning scrap
            scrapWithRarity.rarity = settings.RarityProvider.GetWeight();
        }
        orig(self);
    }
}