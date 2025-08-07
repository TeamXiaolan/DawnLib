using System.Collections.Generic;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.ContentManagement;
using CodeRebirthLib.ContentManagement.Enemies;
using CodeRebirthLib.ContentManagement.Levels;
using CodeRebirthLib.Data;

namespace CodeRebirthLib.Patches;

static class EnemyRegistrationHandler
{
    static readonly Dictionary<string, List<RegistrationSettings<EnemyType>>> _enemiesToInject = [];
    private static readonly Dictionary<SpawnableEnemyWithRarity, RegistrationSettings<EnemyType>> _enemySettingsMap = [];

    internal static void Init()
    {
        On.StartOfRound.Awake += StartOfRound_Awake;
        On.RoundManager.RefreshEnemiesList += RoundManager_RefreshEnemiesList;
        On.QuickMenuManager.Start += QuickMenuManager_Start;
    }

    private static void QuickMenuManager_Start(On.QuickMenuManager.orig_Start orig, QuickMenuManager self) // Make sure you foolproof reloading lobby
    {
        foreach (CREnemyDefinition enemyDefinition in LethalContent.Enemies.CRLib)
        {
            SpawnableEnemyWithRarity spawnableEnemyWithRarity = new SpawnableEnemyWithRarity
            {
                enemyType = enemyDefinition.EnemyType,
                rarity = 1
            };

            if (enemyDefinition.SpawnTable.HasFlag(SpawnTable.Daytime))
            {
                self.testAllEnemiesLevel.DaytimeEnemies.Add(spawnableEnemyWithRarity);
            }
            else if (enemyDefinition.SpawnTable.HasFlag(SpawnTable.Outside))
            {
                self.testAllEnemiesLevel.OutsideEnemies.Add(spawnableEnemyWithRarity);
            }
            else if (enemyDefinition.SpawnTable.HasFlag(SpawnTable.Inside))
            {
                self.testAllEnemiesLevel.Enemies.Add(spawnableEnemyWithRarity);
            }
        }
        orig(self);
    }

    internal static void AddEnemyForLevel(string levelName, RegistrationSettings<EnemyType> settings)
    {
        if (!_enemiesToInject.TryGetValue(levelName, out List<RegistrationSettings<EnemyType>> enemyTypes))
        {
            enemyTypes = new();
        }
        enemyTypes.Add(settings);
        _enemiesToInject[levelName] = enemyTypes;
    }

    private static void StartOfRound_Awake(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        orig(self);
        foreach (SelectableLevel level in StartOfRound.Instance.levels)
        {
            List<RegistrationSettings<EnemyType>> enemies = [];
            
            if (_enemiesToInject.TryGetValue("All", out List<RegistrationSettings<EnemyType>> global))
                enemies.AddRange(global);
            
            // todo: is this where the proper GetLLLMoonName should be used?
            if (_enemiesToInject.TryGetValue(ConfigManager.GetLLLNameOfLevel(level.name), out List<RegistrationSettings<EnemyType>> moonSpecific))
                enemies.AddRange(moonSpecific);

            foreach (RegistrationSettings<EnemyType> enemy in enemies)
            {
                SpawnableEnemyWithRarity spawnDef = new SpawnableEnemyWithRarity
                {
                    enemyType = enemy.Value,
                    rarity = enemy.RarityProvider.GetWeight() // get an inital weight, incase a mod doesn't use any special code.
                };
                
                // todo: xu you talked about wanting to register one enemy as daytime/outside/inside but this only registers as one?
                if (enemy.Value.isDaytimeEnemy && enemy.Value.isOutsideEnemy)
                {
                    level.DaytimeEnemies.Add(spawnDef);
                }
                else if (enemy.Value.isOutsideEnemy)
                {
                    level.OutsideEnemies.Add(spawnDef);
                }
                else
                {
                    level.Enemies.Add(spawnDef);
                }
                
                _enemySettingsMap[spawnDef] = enemy;
            }
        }
    }

    private static void RoundManager_RefreshEnemiesList(On.RoundManager.orig_RefreshEnemiesList orig, RoundManager self)
    {
        UpdateAllWeights();
        orig(self);
    }
    
    internal static void UpdateAllWeights(SelectableLevel? level = null)
    {
        level ??= StartOfRound.Instance.currentLevel;

        foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in level.GetUsedEnemyTypes())
        {
            if (!_enemySettingsMap.TryGetValue(spawnableEnemyWithRarity, out RegistrationSettings<EnemyType> settings))
                continue;

            spawnableEnemyWithRarity.rarity = settings.GetWeight();
        }
    }
}