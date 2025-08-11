using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeRebirthLib;

static class EnemyRegistrationHandler
{   
    internal static void Init()
    {
        On.RoundManager.RefreshEnemiesList += UpdateEnemyWeights;
        On.StartOfRound.SetPlanetsWeather += UpdateEnemyWeights;
        On.StartOfRound.Awake += CollectLevels;
        On.StartOfRound.Awake += RegisterEnemies;
    }

    private static void UpdateEnemyWeights(On.RoundManager.orig_RefreshEnemiesList orig, RoundManager self)
    {
        UpdateEnemyWeightsOnLevel(self.currentLevel);
        orig(self);
    }

    private static void UpdateEnemyWeights(On.StartOfRound.orig_SetPlanetsWeather orig, StartOfRound self, int connectedPlayersOnServer)
    {
        orig(self, connectedPlayersOnServer);
        UpdateEnemyWeightsOnLevel(self.currentLevel);
    }

    internal static void UpdateEnemyWeightsOnLevel(SelectableLevel level)
    {
        foreach (CREnemyInfo enemyInfo in LethalContent.Enemies.Values)
        {
            if (enemyInfo.Key.IsVanilla())
                continue;

            if (enemyInfo.OutsideWeights != null)
            {
                level.OutsideEnemies.Where(x => x.enemyType == enemyInfo.Enemy).First().rarity = enemyInfo.OutsideWeights.GetFor(LethalContent.Moons[level.ToNamespacedKey()]) ?? 0;
            }

            if (enemyInfo.InsideWeights != null)
            {
                level.Enemies.Where(x => x.enemyType == enemyInfo.Enemy).First().rarity = enemyInfo.InsideWeights.GetFor(LethalContent.Moons[level.ToNamespacedKey()]) ?? 0;
            }

            if (enemyInfo.DaytimeWeights != null)
            {
                level.DaytimeEnemies.Where(x => x.enemyType == enemyInfo.Enemy).First().rarity = enemyInfo.DaytimeWeights.GetFor(LethalContent.Moons[level.ToNamespacedKey()]) ?? 0;
            }
        }
    }

    private static void CollectLevels(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        if (LethalContent.Moons.IsFrozen)
        {
            orig(self);
            return;
        }

        foreach (SelectableLevel level in self.levels)
        {
            NamespacedKey<CRMoonInfo> key = level.ToNamespacedKey();
            CRMoonInfo moonInfo = new CRMoonInfo(key, level);
            LethalContent.Moons.Register(moonInfo);
        }

        LethalContent.Moons.Freeze();
        orig(self);
    }
    
    private static void RegisterEnemies(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        if (LethalContent.Enemies.IsFrozen)
        {
            orig(self);
            return;
        }
        
        foreach (SelectableLevel level in self.levels)
        {
            NamespacedKey<CRMoonInfo> moonKey = level.ToNamespacedKey();
            
            List<SpawnableEnemyWithRarity> levelEnemies =
            [
                .. level.Enemies,
                .. level.OutsideEnemies,
                .. level.DaytimeEnemies,
            ];

            foreach (SpawnableEnemyWithRarity enemy in levelEnemies)
            {
                NamespacedKey<CREnemyInfo>? key = (NamespacedKey<CREnemyInfo>?)typeof(EnemyKeys).GetField(enemy.enemyType.enemyName.Replace(" ", ""))?.GetValue(null);
                if (key == null)
                    continue;

                if (LethalContent.Enemies.ContainsKey(key))
                    continue;

                // todo: do weight calculation stuff
                CREnemyInfo enemyInfo = new(key, enemy.enemyType, null, null, null);
                LethalContent.Enemies.Register(enemyInfo);
            }
            
            foreach (CREnemyInfo enemyInfo in LethalContent.Enemies.Values)
            {
                if (enemyInfo.Key.IsVanilla())
                    continue; // also ensure not to register vanilla stuff again

                if(enemyInfo.OutsideWeights != null)
                    TryAddToEnemyList(enemyInfo, level.OutsideEnemies);
                if(enemyInfo.DaytimeWeights != null)
                    TryAddToEnemyList(enemyInfo, level.DaytimeEnemies);
                if(enemyInfo.InsideWeights != null)
                    TryAddToEnemyList(enemyInfo, level.Enemies);
            }
        }
        
        LethalContent.Enemies.Freeze();
        orig(self);
    }
    private static void TryAddToEnemyList(CREnemyInfo enemyInfo, List<SpawnableEnemyWithRarity> list)
    {
        SpawnableEnemyWithRarity spawnDef = new()
        {
            enemyType = enemyInfo.Enemy,
            rarity = 0 // todo: dynamic update
        };
        list.Add(spawnDef);
        
        // todo: keep track of spawnDef to later update the rarity.
    }
}