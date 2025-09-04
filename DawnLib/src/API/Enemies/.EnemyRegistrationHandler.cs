using System.Collections.Generic;
using System.Linq;
using Dawn.Internal;
using UnityEngine;

namespace Dawn;

static class EnemyRegistrationHandler
{
    internal static void Init()
    {
        LethalContent.Enemies.AddAutoTaggers(
            new SimpleAutoTagger<DawnEnemyInfo>(Tags.Killable, info => info.EnemyType.canDie),
            new SimpleAutoTagger<DawnEnemyInfo>(Tags.Small, info => info.EnemyType.EnemySize == EnemySize.Tiny),
            new SimpleAutoTagger<DawnEnemyInfo>(Tags.Medium, info => info.EnemyType.EnemySize == EnemySize.Medium),
            new SimpleAutoTagger<DawnEnemyInfo>(Tags.Large, info => info.EnemyType.EnemySize == EnemySize.Giant)
        );

        On.RoundManager.RefreshEnemiesList += UpdateEnemyWeights;
        On.StartOfRound.SetPlanetsWeather += UpdateEnemyWeights;
        On.EnemyAI.Start += EnsureCorrectEnemyVariables;
        LethalContent.Moons.OnFreeze += RegisterEnemies;
        On.QuickMenuManager.Start += AddEnemiesToDebugList;
        On.Terminal.Awake += AddBestiaryNodes;
    }

    private static void AddBestiaryNodes(On.Terminal.orig_Awake orig, Terminal self)
    {
        TerminalKeyword infoKeyword = self.terminalNodes.allKeywords.First(it => it.word == "info");
        List<TerminalKeyword> allKeywords = self.terminalNodes.allKeywords.ToList();
        List<CompatibleNoun> itemInfoNouns = infoKeyword.compatibleNouns.ToList();

        foreach (DawnEnemyInfo enemyInfo in LethalContent.Enemies.Values)
        {
            if (enemyInfo.HasTag(DawnLibTags.IsExternal) || !enemyInfo.BestiaryNode || !enemyInfo.NameKeyword)
                continue;

            enemyInfo.BestiaryNode.creatureFileID = self.enemyFiles.Count;
            self.enemyFiles.Add(enemyInfo.BestiaryNode);

            ScanNodeProperties[] scanNodes = enemyInfo.EnemyType.enemyPrefab.GetComponentsInChildren<ScanNodeProperties>();
            foreach (ScanNodeProperties scanNode in scanNodes)
            {
                scanNode.creatureScanID = enemyInfo.BestiaryNode.creatureFileID;
            }

            if (allKeywords.Contains(enemyInfo.NameKeyword))
                continue;

            enemyInfo.NameKeyword.defaultVerb = infoKeyword;
            allKeywords.Add(enemyInfo.NameKeyword);
            itemInfoNouns.Add(new CompatibleNoun()
            {
                noun = enemyInfo.NameKeyword,
                result = enemyInfo.BestiaryNode
            });
        }

        infoKeyword.compatibleNouns = itemInfoNouns.ToArray();
        self.terminalNodes.allKeywords = allKeywords.ToArray();
        orig(self);
    }

    private static void AddEnemiesToDebugList(On.QuickMenuManager.orig_Start orig, QuickMenuManager self)
    {
        SelectableLevel testLevel = LethalContent.Moons[MoonKeys.Test].Level;
        foreach (DawnEnemyInfo enemyInfo in LethalContent.Enemies.Values)
        {
            if (enemyInfo.HasTag(DawnLibTags.IsExternal))
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
        if (!self.enemyType.TryGetDawnInfo(out DawnEnemyInfo? enemyInfo))
            return;

        if (enemyInfo.HasTag(DawnLibTags.IsExternal) || StarlancerAIFixCompat.Enabled)
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
        if (!LethalContent.Enemies.IsFrozen || StartOfRound.Instance == null || (WeatherRegistryCompat.Enabled && WeatherRegistryCompat.IsWeatherManagerReady()))
            return;

        foreach (DawnEnemyInfo enemyInfo in LethalContent.Enemies.Values)
        {
            if (enemyInfo.HasTag(DawnLibTags.IsExternal))
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
        Dictionary<EnemyType, WeightTableBuilder<DawnMoonInfo>> enemyInsideWeightBuilder = new();
        Dictionary<EnemyType, WeightTableBuilder<DawnMoonInfo>> enemyOutsideWeightBuilder = new();
        Dictionary<EnemyType, WeightTableBuilder<DawnMoonInfo>> enemyDaytimeWeightBuilder = new();

        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            SelectableLevel level = moonInfo.Level;
            NamespacedKey<DawnMoonInfo> moonKey = moonInfo.TypedKey;

            foreach (var enemyWithRarity in level.Enemies)
            {
                if (enemyWithRarity.enemyType == null)
                    continue;

                if (!enemyInsideWeightBuilder.TryGetValue(enemyWithRarity.enemyType, out WeightTableBuilder<DawnMoonInfo> weightTableBuilder))
                {
                    weightTableBuilder = new WeightTableBuilder<DawnMoonInfo>();
                    enemyInsideWeightBuilder[enemyWithRarity.enemyType] = weightTableBuilder;
                }
                weightTableBuilder.AddWeight(moonKey, enemyWithRarity.rarity);
            }

            foreach (var enemyWithRarity in level.OutsideEnemies)
            {
                if (enemyWithRarity.enemyType == null)
                    continue;

                if (!enemyOutsideWeightBuilder.TryGetValue(enemyWithRarity.enemyType, out WeightTableBuilder<DawnMoonInfo> weightTableBuilder))
                {
                    weightTableBuilder = new WeightTableBuilder<DawnMoonInfo>();
                    enemyOutsideWeightBuilder[enemyWithRarity.enemyType] = weightTableBuilder;
                }
                weightTableBuilder.AddWeight(moonKey, enemyWithRarity.rarity);
            }

            foreach (var enemyWithRarity in level.DaytimeEnemies)
            {
                if (enemyWithRarity.enemyType == null)
                    continue;

                if (!enemyDaytimeWeightBuilder.TryGetValue(enemyWithRarity.enemyType, out WeightTableBuilder<DawnMoonInfo> weightTableBuilder))
                {
                    weightTableBuilder = new WeightTableBuilder<DawnMoonInfo>();
                    enemyDaytimeWeightBuilder[enemyWithRarity.enemyType] = weightTableBuilder;
                }
                weightTableBuilder.AddWeight(moonKey, enemyWithRarity.rarity);
            }
        }

        Terminal terminal = GameObject.FindFirstObjectByType<Terminal>();
        TerminalKeyword infoKeyword = terminal.terminalNodes.allKeywords.First(it => it.word == "info");

        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
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
                if (enemyType == null || enemyType.enemyPrefab == null)
                    continue;

                if (enemyType.TryGetDawnInfo(out _))
                    continue;

                string name = NamespacedKey.NormalizeStringForNamespacedKey(enemyType.enemyName, true);
                NamespacedKey<DawnEnemyInfo>? key = EnemyKeys.GetByReflection(name);
                if (key == null && LethalLibCompat.Enabled && LethalLibCompat.TryGetEnemyTypeFromLethalLib(enemyType, out string lethalLibModName))
                {
                    key = NamespacedKey<DawnEnemyInfo>.From(NamespacedKey.NormalizeStringForNamespacedKey(lethalLibModName, false), NamespacedKey.NormalizeStringForNamespacedKey(enemyType.enemyName, false));
                }
                else if (key == null && LethalLevelLoaderCompat.Enabled && LethalLevelLoaderCompat.TryGetExtendedEnemyTypeModName(enemyType, out string lethalLevelLoaderModName))
                {
                    key = NamespacedKey<DawnEnemyInfo>.From(NamespacedKey.NormalizeStringForNamespacedKey(lethalLevelLoaderModName, false), NamespacedKey.NormalizeStringForNamespacedKey(enemyType.enemyName, false));
                }
                else if (key == null)
                {
                    key = NamespacedKey<DawnEnemyInfo>.From("unknown_lib", NamespacedKey.NormalizeStringForNamespacedKey(enemyType.enemyName, false));
                }

                if (LethalContent.Enemies.ContainsKey(key))
                {
                    DawnPlugin.Logger.LogWarning($"Enemy {enemyType.enemyName} is already registered by the same creator to LethalContent. Skipping...");
                    continue;
                }

                if (!enemyType.enemyPrefab)
                {
                    DawnPlugin.Logger.LogWarning($"{enemyType.enemyName} ({enemyType.name}) didn't have a spawn prefab?");
                    continue;
                }

                DawnEnemyLocationInfo? insideInfo = null;
                DawnEnemyLocationInfo? outsideInfo = null;
                DawnEnemyLocationInfo? daytimeInfo = null;

                if (enemyInsideWeightBuilder.ContainsKey(enemyType))
                {
                    insideInfo = new DawnEnemyLocationInfo(enemyInsideWeightBuilder[enemyType].Build());
                }

                if (enemyOutsideWeightBuilder.ContainsKey(enemyType))
                {
                    outsideInfo = new DawnEnemyLocationInfo(enemyOutsideWeightBuilder[enemyType].Build());
                }

                if (enemyDaytimeWeightBuilder.ContainsKey(enemyType))
                {
                    daytimeInfo = new DawnEnemyLocationInfo(enemyDaytimeWeightBuilder[enemyType].Build());
                }

                List<NamespacedKey> tags = [DawnLibTags.IsExternal];
                if (LethalLevelLoaderCompat.Enabled && LethalLevelLoaderCompat.TryGetAllTagsWithModNames(enemyType, out List<(string modName, string tagName)> tagsWithModNames))
                {
                    foreach ((string modName, string tagName) in tagsWithModNames)
                    {
                        string normalizedModName = NamespacedKey.NormalizeStringForNamespacedKey(modName, false);
                        string normalizedTagName = NamespacedKey.NormalizeStringForNamespacedKey(tagName, false);

                        if (normalizedModName == "lethalcompany")
                        {
                            normalizedModName = "lethal_level_loader";
                        }
                        Debuggers.Enemies?.Log($"Adding tag {normalizedModName}:{normalizedTagName} to enemy {enemyType.enemyName}");
                        tags.Add(NamespacedKey.From(normalizedModName, normalizedTagName));
                    }
                }

                TerminalNode? bestiaryNode = null;
                TerminalKeyword? nameKeyword = null;

                ScanNodeProperties scanNodeProperties = enemyType.enemyPrefab.GetComponentInChildren<ScanNodeProperties>();
                if (scanNodeProperties != null)
                {
                    int creatureScanID = scanNodeProperties.creatureScanID;
                    foreach (CompatibleNoun compatibleNoun in infoKeyword.compatibleNouns)
                    {
                        if (compatibleNoun.result.creatureFileID != creatureScanID)
                            continue;

                        bestiaryNode = compatibleNoun.result;
                        nameKeyword = compatibleNoun.noun;
                    }
                }

                DawnEnemyInfo enemyInfo = new(
                    key, tags,
                    enemyType,
                    outsideInfo, insideInfo, daytimeInfo,
                    bestiaryNode, nameKeyword
                );
                enemyType.SetDawnInfo(enemyInfo);
                LethalContent.Enemies.Register(enemyInfo);
            }

            foreach (DawnEnemyInfo enemyInfo in LethalContent.Enemies.Values)
            {
                if (enemyInfo.HasTag(DawnLibTags.IsExternal))
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

    private static void TryAddToEnemyList(DawnEnemyInfo enemyInfo, List<SpawnableEnemyWithRarity> list)
    {
        SpawnableEnemyWithRarity spawnDef = new()
        {
            enemyType = enemyInfo.EnemyType,
            rarity = 0
        };
        list.Add(spawnDef);
    }
}