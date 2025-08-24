using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CodeRebirthLib.Internal;
using CodeRebirthLib.Internal.ModCompats;
using UnityEngine;

namespace CodeRebirthLib;

static class EnemyRegistrationHandler
{
    internal static void Init()
    {
        On.RoundManager.RefreshEnemiesList += UpdateEnemyWeights;
        On.StartOfRound.SetPlanetsWeather += UpdateEnemyWeights;
        On.EnemyAI.Start += EnsureCorrectEnemyVariables;
        LethalContent.Moons.OnFreeze += RegisterEnemies;
        On.QuickMenuManager.Start += AddEnemiesToDebugList;
    }

    private static void AddEnemiesToDebugList(On.QuickMenuManager.orig_Start orig, QuickMenuManager self)
    {
        SelectableLevel testLevel = LethalContent.Moons[MoonKeys.Test].Level;
        foreach (CREnemyInfo enemyInfo in LethalContent.Enemies.Values)
        {
            if (enemyInfo.Key.IsVanilla() || enemyInfo.HasTag(CRLibTags.IsExternal))
                continue;

            SpawnableEnemyWithRarity spawnDef = new()
            {
                enemyType = enemyInfo.EnemyType,
                rarity = 0
            };

            if (enemyInfo.Inside != null && testLevel.Enemies.All(enemy => enemy.enemyType != enemyInfo.EnemyType))
            {
                Debuggers.Enemies?.Log($"Adding {enemyInfo.EnemyType} to test level {testLevel.name} inside.");
                testLevel.Enemies.Add(spawnDef);
            }

            if (enemyInfo.Outside != null && testLevel.OutsideEnemies.All(enemy => enemy.enemyType != enemyInfo.EnemyType))
            {
                Debuggers.Enemies?.Log($"Adding {enemyInfo.EnemyType} to test level {testLevel.name} outside.");
                testLevel.OutsideEnemies.Add(spawnDef);
            }

            if (enemyInfo.Daytime != null && testLevel.DaytimeEnemies.All(enemy => enemy.enemyType != enemyInfo.EnemyType))
            {
                Debuggers.Enemies?.Log($"Adding {enemyInfo.EnemyType} to test level {testLevel.name} daytime.");
                testLevel.DaytimeEnemies.Add(spawnDef);
            }
        }
        orig(self);
    }

    private static void EnsureCorrectEnemyVariables(On.EnemyAI.orig_Start orig, EnemyAI self)
    {
        if (!self.enemyType.TryGetCRInfo(out CREnemyInfo? enemyInfo))
            return;

        if (enemyInfo.HasTag(CRLibTags.IsExternal) || StarlancerAIFixCompat.Enabled)
        {
            orig(self);
            return;
        }

        if (enemyInfo.Daytime != null)
        {
            self.enemyType.isDaytimeEnemy = true;
        }
        GameObject[]? insideNodes = RoundManager.Instance.insideAINodes;
        GameObject[]? outsideNodes = RoundManager.Instance.outsideAINodes;
        bool insideIsClosest = true;

        float closestDistance = float.MaxValue;
        if (insideNodes != null)
        {
            foreach (var node in insideNodes)
            {
                float distance = Vector3.Distance(node.transform.position, self.transform.position);
                if (distance >= closestDistance)
                    continue;

                closestDistance = distance;
            }
        }
        if (outsideNodes != null)
        {
            foreach (var node in outsideNodes)
            {
                float distance = Vector3.Distance(node.transform.position, self.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    insideIsClosest = false;
                    break;
                }
            }
        }

        bool previouslyOutside = self.enemyType.isOutsideEnemy;
        if (insideIsClosest)
        {
            self.enemyType.isOutsideEnemy = false;
        }
        else
        {
            self.enemyType.isOutsideEnemy = true;
        }

        orig(self);

        if (previouslyOutside != self.enemyType.isOutsideEnemy)
        {
            self.enemyType.isOutsideEnemy = previouslyOutside;
        }
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
        if (!LethalContent.Enemies.IsFrozen)
            return;

        foreach (CREnemyInfo enemyInfo in LethalContent.Enemies.Values)
        {
            if (enemyInfo.Key.IsVanilla() || enemyInfo.HasTag(CRLibTags.IsExternal))
                continue;

            Debuggers.Enemies?.Log($"Updating weights for {enemyInfo.EnemyType} on level {level.PlanetName}");
            if (enemyInfo.Outside != null)
            {
                level.OutsideEnemies.Where(x => x.enemyType == enemyInfo.EnemyType).First().rarity = enemyInfo.Outside.Weights.GetFor(LethalContent.Moons[level.ToNamespacedKey()]) ?? 0;
            }

            if (enemyInfo.Inside != null)
            {
                level.Enemies.Where(x => x.enemyType == enemyInfo.EnemyType).First().rarity = enemyInfo.Inside.Weights.GetFor(LethalContent.Moons[level.ToNamespacedKey()]) ?? 0;
            }

            if (enemyInfo.Daytime != null)
            {
                level.DaytimeEnemies.Where(x => x.enemyType == enemyInfo.EnemyType).First().rarity = enemyInfo.Daytime.Weights.GetFor(LethalContent.Moons[level.ToNamespacedKey()]) ?? 0;
            }
        }
    }

    private static void RegisterEnemies()
    {
        Dictionary<EnemyType, WeightTableBuilder<CRMoonInfo>> enemyInsideWeightBuilder = new();
        Dictionary<EnemyType, WeightTableBuilder<CRMoonInfo>> enemyOutsideWeightBuilder = new();
        Dictionary<EnemyType, WeightTableBuilder<CRMoonInfo>> enemyDaytimeWeightBuilder = new();

        foreach (CRMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            SelectableLevel level = moonInfo.Level;
            NamespacedKey<CRMoonInfo> moonKey = moonInfo.TypedKey;

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

        foreach (CRMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            SelectableLevel level = moonInfo.Level;
            List<SpawnableEnemyWithRarity> levelEnemies =
            [
                .. level.Enemies,
                .. level.OutsideEnemies,
                .. level.DaytimeEnemies,
            ];

            foreach (SpawnableEnemyWithRarity enemyWithRarity in levelEnemies)
            {
                EnemyType? enemyType = enemyWithRarity.enemyType;
                if (enemyType == null)
                    continue;

                if (enemyType.TryGetCRInfo(out _))
                    continue;

                NamespacedKey<CREnemyInfo>? key = (NamespacedKey<CREnemyInfo>?)typeof(EnemyKeys).GetField(NamespacedKey.NormalizeStringForNamespacedKey(enemyType.enemyName, true))?.GetValue(null);
                key ??= NamespacedKey<CREnemyInfo>.From("modded_please_replace_this_later", NamespacedKey.NormalizeStringForNamespacedKey(enemyType.enemyName, false));

                WeightTableBuilder<CRMoonInfo> insideWeightBuilder = new();
                WeightTableBuilder<CRMoonInfo> outsideWeightBuilder = new();
                WeightTableBuilder<CRMoonInfo> daytimeWeightBuilder = new();

                if (enemyInsideWeightBuilder.ContainsKey(enemyType))
                {
                    insideWeightBuilder = enemyInsideWeightBuilder[enemyType];
                }
                if (enemyOutsideWeightBuilder.ContainsKey(enemyType))
                {
                    outsideWeightBuilder = enemyOutsideWeightBuilder[enemyType];
                }
                if (enemyDaytimeWeightBuilder.ContainsKey(enemyType))
                {
                    daytimeWeightBuilder = enemyDaytimeWeightBuilder[enemyType];
                }

                List<NamespacedKey> tags = [CRLibTags.IsExternal];
                if (enemyType.canDie)
                {
                    tags.Add(Tags.Killable);
                }

                if (enemyType.EnemySize == EnemySize.Tiny)
                {
                    tags.Add(Tags.Small);
                }
                else if (enemyType.EnemySize == EnemySize.Medium)
                {
                    tags.Add(Tags.Medium);
                }
                else
                {
                    tags.Add(Tags.Large);
                }
    
                CREnemyInfo enemyInfo = new(key, tags, enemyType, new CREnemyLocationInfo(insideWeightBuilder.Build()), new CREnemyLocationInfo(outsideWeightBuilder.Build()), new CREnemyLocationInfo(daytimeWeightBuilder.Build()));
                enemyType.SetCRInfo(enemyInfo);
                LethalContent.Enemies.Register(enemyInfo);
            }

            foreach (CREnemyInfo enemyInfo in LethalContent.Enemies.Values)
            {
                if (enemyInfo.Key.IsVanilla() || enemyInfo.HasTag(CRLibTags.IsExternal))
                    continue;

                if (enemyInfo.Outside != null)
                    TryAddToEnemyList(enemyInfo, level.OutsideEnemies);

                if (enemyInfo.Daytime != null)
                    TryAddToEnemyList(enemyInfo, level.DaytimeEnemies);

                if (enemyInfo.Inside != null)
                    TryAddToEnemyList(enemyInfo, level.Enemies);
            }
        }
        LethalContent.Enemies.Freeze();
    }

    private static void TryAddToEnemyList(CREnemyInfo enemyInfo, List<SpawnableEnemyWithRarity> list)
    {
        SpawnableEnemyWithRarity spawnDef = new()
        {
            enemyType = enemyInfo.EnemyType,
            rarity = 0
        };
        list.Add(spawnDef);
    }
}