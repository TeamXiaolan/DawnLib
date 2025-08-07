using System.Collections.Generic;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.Data;

namespace CodeRebirthLib.Patches;
static class ItemRegistrationHandler
{
    private static readonly List<Item> _allNewItems = [];
    static readonly Dictionary<string, List<RegistrationSettings<Item>>> _itemsToInject = [];
    private static readonly Dictionary<SpawnableItemWithRarity, RegistrationSettings<Item>> _itemSettingsMap = [];

    internal static void Init()
    {
        On.StartOfRound.Awake += StartOfRound_Awake;
        On.RoundManager.SpawnScrapInLevel += RoundManager_SpawnScrapInLevel;
    }

    internal static void AddItemForLevel(string levelName, RegistrationSettings<Item> settings)
    {
        if (!_itemsToInject.TryGetValue(levelName, out List<RegistrationSettings<Item>> items))
        {
            items = new();
        }
        items.Add(settings);
        _itemsToInject[levelName] = items;
        AddItemToAllList(settings.Value);
    }

    internal static void AddItemToAllList(Item item)
    {
        if (!_allNewItems.Contains(item))
            _allNewItems.Add(item);
    }

    private static void StartOfRound_Awake(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        orig(self);
        foreach (SelectableLevel level in StartOfRound.Instance.levels)
        {
            List<RegistrationSettings<Item>> items = [];
            
            if (_itemsToInject.TryGetValue("All", out List<RegistrationSettings<Item>> globalItems))
                items.AddRange(globalItems);
            
            if (_itemsToInject.TryGetValue(ConfigManager.GetLLLNameOfLevel(level.name), out List<RegistrationSettings<Item>> moonSpecificItems))
                items.AddRange(moonSpecificItems);

            foreach (RegistrationSettings<Item> item in items)
            {
                SpawnableItemWithRarity spawnDef = new SpawnableItemWithRarity
                {
                    spawnableItem = item.Value,
                    rarity = item.RarityProvider.GetWeight() // get an inital weight, incase a mod doesn't use any special code.
                };
                level.spawnableScrap.Add(spawnDef);
                _itemSettingsMap[spawnDef] = item;
            }
        }

        foreach (Item item in _allNewItems)
        {
            self.allItemsList.itemsList.Add(item);
        }
        
        _itemsToInject.Clear();
    }

    private static void RoundManager_SpawnScrapInLevel(On.RoundManager.orig_SpawnScrapInLevel orig, RoundManager self)
    {
        foreach (SpawnableItemWithRarity scrapWithRarity in self.currentLevel.spawnableScrap)
        {
            if(!_itemSettingsMap.TryGetValue(scrapWithRarity, out RegistrationSettings<Item> settings))
                continue;

            // update weights just before spawning scrap
            scrapWithRarity.rarity = settings.RarityProvider.GetWeight();
        }
        orig(self);
    }
}