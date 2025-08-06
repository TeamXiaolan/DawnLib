using System.Collections.Generic;
using CodeRebirthLib.ConfigManagement.Weights;
using CodeRebirthLib.ContentManagement;
using CodeRebirthLib.ContentManagement.Enemies;
using CodeRebirthLib.ContentManagement.Items;

namespace CodeRebirthLib.Patches;

static class CREnemiesPatch
{
    static readonly Dictionary<SpawnWeightsPreset, List<EnemyType>> enemiesToInjectThroughPreset = [];

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

    internal static void AddEnemyForLevel(SpawnTable spawnTable, SpawnWeightsPreset spawnWeightsPreset, EnemyType enemyType)
    {
        if (!enemiesToInjectThroughPreset.TryGetValue(spawnWeightsPreset, out List<EnemyType> enemyTypes))
        {
            enemyTypes = new();
        }
        enemyTypes.Add(enemyType);
        enemiesToInjectThroughPreset[spawnWeightsPreset] = enemyTypes;
    }

    private static void StartOfRound_Awake(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        orig(self);
        foreach (var spawnPresetsWithEnemiesIn in enemiesToInjectThroughPreset)
        {
            foreach (SelectableLevel level in StartOfRound.Instance.levels)
            {
                foreach (EnemyType enemy in spawnPresetsWithEnemiesIn.Value)
                {
                    if (!enemy.TryGetDefinition(out _))
                        continue;

                    var spawnableEnemyWithRarity = new SpawnableEnemyWithRarity()
                    {
                        enemyType = enemy,
                        rarity = 0
                    };

                    if (enemy.isDaytimeEnemy && enemy.isOutsideEnemy)
                    {
                        level.DaytimeEnemies.Add(spawnableEnemyWithRarity);
                    }
                    else if (enemy.isOutsideEnemy)
                    {
                        level.OutsideEnemies.Add(spawnableEnemyWithRarity);
                    }
                    else
                    {
                        level.Enemies.Add(spawnableEnemyWithRarity);
                    }
                }
            }
        }
    }

    private static void RoundManager_RefreshEnemiesList(On.RoundManager.orig_RefreshEnemiesList orig, RoundManager self)
    {
        CREnemyDefinition.UpdateAllWeights();
        orig(self);
    }
}