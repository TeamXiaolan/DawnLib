using System.Collections.Generic;
using System.Linq;
using Dawn.Internal;
using MonoMod.RuntimeDetour;
using Unity.Netcode;
using UnityEngine;

namespace Dawn;

static class EnemyRegistrationHandler
{
    private static List<EnemyType> _networkPrefabEnemyTypes = new();

    internal static void Init()
    {
        LethalContent.Enemies.AddAutoTaggers(
            new SimpleAutoTagger<DawnEnemyInfo>(Tags.Killable, info => info.EnemyType.canDie),
            new SimpleAutoTagger<DawnEnemyInfo>(Tags.Small, info => info.EnemyType.EnemySize == EnemySize.Tiny),
            new SimpleAutoTagger<DawnEnemyInfo>(Tags.Medium, info => info.EnemyType.EnemySize == EnemySize.Medium),
            new SimpleAutoTagger<DawnEnemyInfo>(Tags.Large, info => info.EnemyType.EnemySize == EnemySize.Giant)
        );

        On.RoundManager.RefreshEnemiesList += UpdateEnemyWeights;
        On.RoundManager.AssignRandomEnemyToVent += CheckIfEnemyCanSpawn;
        On.StartOfRound.SetPlanetsWeather += UpdateEnemyWeights;
        On.EnemyAI.Start += EnsureCorrectEnemyVariables;
        On.QuickMenuManager.Start += AddEnemiesToDebugList;

        using (new DetourContext(priority: int.MaxValue))
        {
            On.Terminal.Awake += AddBestiaryNodes;
            On.GameNetworkManager.Start += CollectAllEnemyTypes;
        }

        LethalContent.Moons.OnFreeze += RegisterEnemies;
        LethalContent.Enemies.OnFreeze += RedoEnemiesDebugMenu;
    }

    private static bool CheckIfEnemyCanSpawn(On.RoundManager.orig_AssignRandomEnemyToVent orig, RoundManager self, EnemyVent vent, float spawnTime)
    {
        if (self.enemyRushIndex == -1)
        {
            return orig(self, vent, spawnTime);
        }

        List<EnemyType> enemiesEdited = new();
        foreach (EnemyType enemyType in self.currentLevel.Enemies.Select(def => def.enemyType))
        {
            if (!enemyType.HasDawnInfo())
                continue;

            DawnEnemyInfo enemyInfo = enemyType.GetDawnInfo();
            if (enemyInfo.ShouldSkipRespectOverride())
                continue;

            if (enemyInfo.EnemyType.spawningDisabled)
                continue;

            if (enemyInfo.Inside == null)
                continue;

            SpawnWeightContext ctx = new(self.currentLevel.GetDawnInfo(), self.dungeonGenerator.Generator.DungeonFlow.GetDawnInfo(), TimeOfDayRefs.GetCurrentWeatherEffect(self.currentLevel)?.GetDawnInfo());
            if (enemyInfo.Inside.Weights.GetFor(ctx.Moon!, ctx) != 0)
                continue;

            enemyType.spawningDisabled = true;
            enemiesEdited.Add(enemyType);
        }

        orig(self, vent, spawnTime);
        foreach (EnemyType enemyType in enemiesEdited)
        {
            enemyType.spawningDisabled = false;
        }
        return true;
    }

    private static void RedoEnemiesDebugMenu()
    {
        QuickMenuManagerRefs.Instance.Debug_SetEnemyDropdownOptions();
    }

    private static void CollectAllEnemyTypes(On.GameNetworkManager.orig_Start orig, GameNetworkManager self)
    {
        orig(self);
        foreach (NetworkPrefab networkPrefab in NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs)
        {
            if (!networkPrefab.Prefab.TryGetComponent(out EnemyAI enemyAI))
                continue;

            if (enemyAI.enemyType == null)
            {
                continue;
            }

            if (_networkPrefabEnemyTypes.Contains(enemyAI.enemyType))
            {
                continue;
            }

            _networkPrefabEnemyTypes.Add(enemyAI.enemyType);
        }
    }

    private static void AddBestiaryNodes(On.Terminal.orig_Awake orig, Terminal self)
    {
        foreach (DawnEnemyInfo enemyInfo in LethalContent.Enemies.Values)
        {
            if (enemyInfo.ShouldSkipIgnoreOverride() || !enemyInfo.BestiaryNode || !enemyInfo.NameKeyword)
                continue;

            AddScanNodeToBestiaryEvent(enemyInfo.EnemyType.enemyPrefab, enemyInfo.BestiaryNode!, enemyInfo.NameKeyword!);
        }
        orig(self);
    }

    private static void AddScanNodeToBestiaryEvent(GameObject gameObjectWithScanNodes, TerminalNode bestiaryNode, TerminalKeyword nameKeyword)
    {
        List<TerminalKeyword> allKeywords = TerminalRefs.Instance.terminalNodes.allKeywords.ToList();
        List<CompatibleNoun> itemInfoNouns = TerminalRefs.InfoKeyword.compatibleNouns.ToList();

        bestiaryNode.creatureFileID = TerminalRefs.Instance.enemyFiles.Count;
        TerminalRefs.Instance.enemyFiles.Add(bestiaryNode);

        ScanNodeProperties[] scanNodes = gameObjectWithScanNodes.GetComponentsInChildren<ScanNodeProperties>();
        foreach (ScanNodeProperties scanNode in scanNodes)
        {
            scanNode.creatureScanID = bestiaryNode.creatureFileID;
        }

        if (allKeywords.Contains(nameKeyword))
            return;

        nameKeyword.defaultVerb = TerminalRefs.InfoKeyword;
        allKeywords.Add(nameKeyword);
        itemInfoNouns.Add(new CompatibleNoun()
        {
            noun = nameKeyword,
            result = bestiaryNode
        });

        TerminalRefs.InfoKeyword.compatibleNouns = itemInfoNouns.ToArray();
        TerminalRefs.Instance.terminalNodes.allKeywords = allKeywords.ToArray();
    }

    private static void AddEnemiesToDebugList(On.QuickMenuManager.orig_Start orig, QuickMenuManager self)
    {
        SelectableLevel testLevel = LethalContent.Moons[MoonKeys.Test].Level;
        foreach (DawnEnemyInfo enemyInfo in LethalContent.Enemies.Values)
        {
            if (enemyInfo.ShouldSkipIgnoreOverride())
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
        if (!self.enemyType.HasDawnInfo())
        {
            DawnPlugin.Logger.LogError($"Enemy with names {self.enemyType.name} and {self.enemyType.enemyName} has no DawnEnemyInfo, this means this enemy is not properly registered.");
            orig(self);
            return;
        }

        DawnEnemyInfo enemyInfo = self.enemyType.GetDawnInfo();
        if ((enemyInfo.ShouldSkipRespectOverride()) || StarlancerAIFixCompat.Enabled)
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
                if (node == null)
                    continue;

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
                if (node == null)
                    continue;

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
        if (!LethalContent.Weathers.IsFrozen || !LethalContent.Enemies.IsFrozen || StartOfRound.Instance == null || (WeatherRegistryCompat.Enabled && !WeatherRegistryCompat.IsWeatherManagerReady()))
            return;

        foreach (DawnEnemyInfo enemyInfo in LethalContent.Enemies.Values)
        {
            if (enemyInfo.ShouldSkipRespectOverride())
                continue;

            Debuggers.Enemies?.Log($"Updating weights for {enemyInfo.EnemyType} on level {level.PlanetName}");
            if (enemyInfo.Outside != null)
            {
                SpawnableEnemyWithRarity? spawnableEnemyWithRarity = level.OutsideEnemies.FirstOrDefault(x => x.enemyType == enemyInfo.EnemyType);
                if (spawnableEnemyWithRarity == null)
                {
                    spawnableEnemyWithRarity = new()
                    {
                        enemyType = enemyInfo.EnemyType,
                        rarity = 0
                    };
                    level.OutsideEnemies.Add(spawnableEnemyWithRarity);
                }
                SpawnWeightContext ctx = new(level.GetDawnInfo(), null, TimeOfDayRefs.GetCurrentWeatherEffect(level)?.GetDawnInfo());
                spawnableEnemyWithRarity.rarity = enemyInfo.Outside.Weights.GetFor(ctx.Moon!, ctx) ?? 0;
            }

            if (enemyInfo.Inside != null)
            {
                SpawnableEnemyWithRarity? spawnableEnemyWithRarity = level.Enemies.FirstOrDefault(x => x.enemyType == enemyInfo.EnemyType);
                if (spawnableEnemyWithRarity == null)
                {
                    spawnableEnemyWithRarity = new()
                    {
                        enemyType = enemyInfo.EnemyType,
                        rarity = 0
                    };
                    level.Enemies.Add(spawnableEnemyWithRarity);
                }
                SpawnWeightContext ctx = new(level.GetDawnInfo(), null, TimeOfDayRefs.GetCurrentWeatherEffect(level)?.GetDawnInfo());
                int rarity = enemyInfo.Inside.Weights.GetFor(ctx.Moon!, ctx) ?? 0;
                spawnableEnemyWithRarity.rarity = rarity;
            }

            if (enemyInfo.Daytime != null)
            {
                SpawnableEnemyWithRarity? spawnableEnemyWithRarity = level.DaytimeEnemies.FirstOrDefault(x => x.enemyType == enemyInfo.EnemyType);
                if (spawnableEnemyWithRarity == null)
                {
                    spawnableEnemyWithRarity = new()
                    {
                        enemyType = enemyInfo.EnemyType,
                        rarity = 0
                    };
                    level.DaytimeEnemies.Add(spawnableEnemyWithRarity);
                }

                SpawnWeightContext ctx = new(level.GetDawnInfo(), null, TimeOfDayRefs.GetCurrentWeatherEffect(level)?.GetDawnInfo());
                spawnableEnemyWithRarity.rarity = enemyInfo.Daytime.Weights.GetFor(ctx.Moon!, ctx) ?? 0;
            }
        }
    }

    private static void RegisterEnemies()
    {
        Dictionary<string, WeightTableBuilder<DawnMoonInfo, SpawnWeightContext>> enemyInsideWeightBuilder = new();
        Dictionary<string, WeightTableBuilder<DawnMoonInfo, SpawnWeightContext>> enemyOutsideWeightBuilder = new();
        Dictionary<string, WeightTableBuilder<DawnMoonInfo, SpawnWeightContext>> enemyDaytimeWeightBuilder = new();

        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            SelectableLevel level = moonInfo.Level;
            NamespacedKey<DawnMoonInfo> moonKey = moonInfo.TypedKey;

            foreach (SpawnableEnemyWithRarity enemyWithRarity in level.Enemies)
            {
                if (enemyWithRarity.enemyType == null)
                    continue;

                if (!enemyInsideWeightBuilder.TryGetValue(enemyWithRarity.enemyType.name, out WeightTableBuilder<DawnMoonInfo, SpawnWeightContext> weightTableBuilder))
                {
                    weightTableBuilder = new WeightTableBuilder<DawnMoonInfo, SpawnWeightContext>();
                    enemyInsideWeightBuilder[enemyWithRarity.enemyType.name] = weightTableBuilder;
                }
                Debuggers.Enemies?.Log($"Adding inside weight {enemyWithRarity.rarity} for {enemyWithRarity.enemyType} on level {level.PlanetName}");
                weightTableBuilder.AddWeight(moonKey, enemyWithRarity.rarity);
            }

            foreach (SpawnableEnemyWithRarity enemyWithRarity in level.OutsideEnemies)
            {
                if (enemyWithRarity.enemyType == null)
                    continue;

                if (!enemyOutsideWeightBuilder.TryGetValue(enemyWithRarity.enemyType.name, out WeightTableBuilder<DawnMoonInfo, SpawnWeightContext> weightTableBuilder))
                {
                    weightTableBuilder = new WeightTableBuilder<DawnMoonInfo, SpawnWeightContext>();
                    enemyOutsideWeightBuilder[enemyWithRarity.enemyType.name] = weightTableBuilder;
                }
                Debuggers.Enemies?.Log($"Adding outside weight {enemyWithRarity.rarity} for {enemyWithRarity.enemyType} on level {level.PlanetName}");
                weightTableBuilder.AddWeight(moonKey, enemyWithRarity.rarity);
            }

            foreach (SpawnableEnemyWithRarity enemyWithRarity in level.DaytimeEnemies)
            {
                if (enemyWithRarity.enemyType == null)
                    continue;

                if (!enemyDaytimeWeightBuilder.TryGetValue(enemyWithRarity.enemyType.name, out WeightTableBuilder<DawnMoonInfo, SpawnWeightContext> weightTableBuilder))
                {
                    weightTableBuilder = new WeightTableBuilder<DawnMoonInfo, SpawnWeightContext>();
                    enemyDaytimeWeightBuilder[enemyWithRarity.enemyType.name] = weightTableBuilder;
                }
                Debuggers.Enemies?.Log($"Adding daytime weight {enemyWithRarity.rarity} for {enemyWithRarity.enemyType} on level {level.PlanetName}");
                weightTableBuilder.AddWeight(moonKey, enemyWithRarity.rarity);
            }
        }

        TerminalKeyword infoKeyword = TerminalRefs.InfoKeyword;

        foreach (EnemyType? enemyType in _networkPrefabEnemyTypes)
        {
            if (enemyType == null || enemyType.enemyPrefab == null)
                continue;

            if (enemyType.HasDawnInfo())
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
                DawnPlugin.Logger.LogWarning($"Enemy {enemyType.enemyName} is already registered by the same creator to LethalContent. This is likely to cause issues.");
                enemyType.SetDawnInfo(LethalContent.Enemies[key]);
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

            if (enemyInsideWeightBuilder.ContainsKey(enemyType.name))
            {
                insideInfo = new DawnEnemyLocationInfo(enemyInsideWeightBuilder[enemyType.name].Build());
            }

            if (enemyOutsideWeightBuilder.ContainsKey(enemyType.name))
            {
                outsideInfo = new DawnEnemyLocationInfo(enemyOutsideWeightBuilder[enemyType.name].Build());
            }

            if (enemyDaytimeWeightBuilder.ContainsKey(enemyType.name))
            {
                daytimeInfo = new DawnEnemyLocationInfo(enemyDaytimeWeightBuilder[enemyType.name].Build());
            }

            HashSet<NamespacedKey> tags = [DawnLibTags.IsExternal];
            CollectLLLTags(enemyType, tags);

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
                key, tags, enemyType,
                outsideInfo, insideInfo, daytimeInfo,
                bestiaryNode, nameKeyword,
                null
            );
            enemyType.SetDawnInfo(enemyInfo);
            LethalContent.Enemies.Register(enemyInfo);
        }

        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            SelectableLevel level = moonInfo.Level;
            foreach (DawnEnemyInfo enemyInfo in LethalContent.Enemies.Values)
            {
                if (enemyInfo.ShouldSkipRespectOverride())
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

    private static void CollectLLLTags(EnemyType enemyType, HashSet<NamespacedKey> tags)
    {
        if (LethalLevelLoaderCompat.Enabled && LethalLevelLoaderCompat.TryGetAllTagsWithModNames(enemyType, out List<(string modName, string tagName)> tagsWithModNames))
        {
            tags.AddToList(tagsWithModNames, Debuggers.Enemies, enemyType.name);
        }
    }

    private static void TryAddToEnemyList(DawnEnemyInfo enemyInfo, List<SpawnableEnemyWithRarity> list)
    {
        foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in list)
        {
            if (spawnableEnemyWithRarity.enemyType == enemyInfo.EnemyType)
            {
                return;
            }
        }

        SpawnableEnemyWithRarity spawnDef = new()
        {
            enemyType = enemyInfo.EnemyType,
            rarity = 0
        };
        list.Add(spawnDef);
    }
}