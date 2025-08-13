using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CodeRebirthLib;

static class EnemyRegistrationHandler
{   
    internal static void Init()
    {
        On.RoundManager.RefreshEnemiesList += UpdateEnemyWeights;
        On.StartOfRound.SetPlanetsWeather += UpdateEnemyWeights;
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
    
    private static void RegisterEnemies(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        if (LethalContent.Enemies.IsFrozen)
        {
            orig(self);
            return;
        }

        Dictionary<EnemyType, WeightTableBuilder<CRMoonInfo>> enemyInsideWeightBuilder = new();
        Dictionary<EnemyType, WeightTableBuilder<CRMoonInfo>> enemyOutsideWeightBuilder = new();
        Dictionary<EnemyType, WeightTableBuilder<CRMoonInfo>> enemyDaytimeWeightBuilder = new();

        foreach (var level in self.levels)
        {
            NamespacedKey<CRMoonInfo> moonKey = level.ToNamespacedKey();

            foreach (var enemyWithRarity in level.Enemies)
            {
                if (enemyWithRarity.enemyType == null)
                    continue;

                if (!enemyInsideWeightBuilder.TryGetValue(enemyWithRarity.enemyType, out WeightTableBuilder<CRMoonInfo> weightTableBuilder))
                {
                    weightTableBuilder = new WeightTableBuilder<CRMoonInfo>();
                    enemyInsideWeightBuilder[enemyWithRarity.enemyType] = weightTableBuilder;
                }
                weightTableBuilder.AddWeight(moonKey, enemyWithRarity.rarity);
            }

            foreach (var enemyWithRarity in level.OutsideEnemies)
            {
                if (enemyWithRarity.enemyType == null)
                    continue;

                if (!enemyOutsideWeightBuilder.TryGetValue(enemyWithRarity.enemyType, out WeightTableBuilder<CRMoonInfo> weightTableBuilder))
                {
                    weightTableBuilder = new WeightTableBuilder<CRMoonInfo>();
                    enemyOutsideWeightBuilder[enemyWithRarity.enemyType] = weightTableBuilder;
                }
                weightTableBuilder.AddWeight(moonKey, enemyWithRarity.rarity);
            }

            foreach (var enemyWithRarity in level.DaytimeEnemies)
            {
                if (enemyWithRarity.enemyType == null)
                    continue;

                if (!enemyDaytimeWeightBuilder.TryGetValue(enemyWithRarity.enemyType, out WeightTableBuilder<CRMoonInfo> weightTableBuilder))
                {
                    weightTableBuilder = new WeightTableBuilder<CRMoonInfo>();
                    enemyDaytimeWeightBuilder[enemyWithRarity.enemyType] = weightTableBuilder;
                }
                weightTableBuilder.AddWeight(moonKey, enemyWithRarity.rarity);
            }
        }

        foreach (SelectableLevel level in self.levels)
        {
            List<SpawnableEnemyWithRarity> levelEnemies =
            [
                .. level.Enemies,
                .. level.OutsideEnemies,
                .. level.DaytimeEnemies,
            ];

            foreach (SpawnableEnemyWithRarity enemyWithRarity in levelEnemies)
            {
                if (enemyWithRarity.enemyType == null)
                    continue;

                NamespacedKey<CREnemyInfo>? key = (NamespacedKey<CREnemyInfo>?)typeof(EnemyKeys).GetField(enemyWithRarity.enemyType.enemyName.Replace("-", "_").Replace(" ", "_"))?.GetValue(null);
                if (key == null)
                    continue;

                if (LethalContent.Enemies.ContainsKey(key))
                    continue;

                WeightTableBuilder<CRMoonInfo> insideWeightBuilder = new();
                WeightTableBuilder<CRMoonInfo> outsideWeightBuilder = new();
                WeightTableBuilder<CRMoonInfo> daytimeWeightBuilder = new();

                if (enemyInsideWeightBuilder.ContainsKey(enemyWithRarity.enemyType))
                {
                    insideWeightBuilder = enemyInsideWeightBuilder[enemyWithRarity.enemyType];
                }
                if (enemyOutsideWeightBuilder.ContainsKey(enemyWithRarity.enemyType))
                {
                    outsideWeightBuilder = enemyOutsideWeightBuilder[enemyWithRarity.enemyType];
                }
                if (enemyDaytimeWeightBuilder.ContainsKey(enemyWithRarity.enemyType))
                {
                    daytimeWeightBuilder = enemyDaytimeWeightBuilder[enemyWithRarity.enemyType];
                }

                CREnemyInfo enemyInfo = new(key, enemyWithRarity.enemyType, insideWeightBuilder.Build(), outsideWeightBuilder.Build(), daytimeWeightBuilder.Build());
                LethalContent.Enemies.Register(enemyInfo);
            }

            foreach (CREnemyInfo enemyInfo in LethalContent.Enemies.Values)
            {
                if (enemyInfo.Key.IsVanilla())
                    continue; // also ensure not to register vanilla stuff again

                if (enemyInfo.OutsideWeights != null)
                    TryAddToEnemyList(enemyInfo, level.OutsideEnemies);

                if (enemyInfo.DaytimeWeights != null)
                    TryAddToEnemyList(enemyInfo, level.DaytimeEnemies);

                if (enemyInfo.InsideWeights != null)
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
            rarity = 0
        };
        list.Add(spawnDef);        
    }
}