using System.Collections.Generic;
using System.Linq;
using Dawn.Internal;
using DunGen.Graph;
using Mono.Cecil.Cil;
using MonoMod.Cil;
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

        IL.RoundManager.SpawnRandomDaytimeEnemy += StopDawnEnemyResetting;
        IL.RoundManager.AssignRandomEnemyToVent += StopDawnEnemyResetting;
        IL.RoundManager.SpawnRandomOutsideEnemy += StopDawnEnemyResetting;
        IL.RoundManager.SpawnRandomWeedEnemy += StopDawnEnemyResetting;
        IL.RoundManager.PredictAllOutsideEnemies += StopDawnEnemyResetting;

        using (new DetourContext(priority: int.MaxValue))
        {
            On.Terminal.Awake += AddBestiaryNodes;
            On.GameNetworkManager.Start += CollectAllEnemyTypes;
            On.Terminal.Start += GrabModdedBestiaryNodes;
        }

        LethalContent.Moons.OnFreezeWithContext += _ => RegisterEnemies();
        LethalContent.Enemies.OnFreezeWithContext += _ => RedoEnemiesDebugMenu();
        LethalContent.Enemies.OnFreezeWithContext += _ => FixDawnEnemyReferences();
    }

    private static void GrabModdedBestiaryNodes(On.Terminal.orig_Start orig, Terminal self)
    {
        orig(self);
        foreach (DawnEnemyInfo enemyInfo in LethalContent.Enemies.Values)
        {
            ScanNodeProperties scanNodeProperties = enemyInfo.EnemyType.enemyPrefab.GetComponentInChildren<ScanNodeProperties>();
            if (scanNodeProperties != null)
            {
                int creatureScanID = scanNodeProperties.creatureScanID;
                foreach (CompatibleNoun compatibleNoun in TerminalRefs.InfoKeyword.compatibleNouns)
                {
                    if (compatibleNoun.result.creatureFileID != creatureScanID)
                        continue;

                    enemyInfo.BestiaryNode = compatibleNoun.result;
                    enemyInfo.NameKeyword = compatibleNoun.noun;
                    break;
                }
            }
        }
    }

    private static void StopDawnEnemyResetting(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        int firstEnemyTypeLoc = -1;
        ILLabel firstSkip = null!;
        if (!c.TryGotoNext(
            MoveType.After,
            c => c.MatchLdfld<SpawnableEnemyWithRarity>(nameof(SpawnableEnemyWithRarity.enemyType)),
            c => c.MatchStloc(out firstEnemyTypeLoc)
        ))
        {
            DawnPlugin.Logger.LogError($"Failed to apply {il.Method.Name} patch (0)!");
            return;
        }

        if (!c.TryGotoNext(
            MoveType.After,
            c => c.MatchBrfalse(out firstSkip)
        ))
        {
            DawnPlugin.Logger.LogError($"Failed to apply {il.Method.Name} patch (1)!");
            return;
        }

        // emit function that takes in enemytype and return true or false, false to do vanilla behaviour
        c.Emit(OpCodes.Ldloc, firstEnemyTypeLoc);
        c.EmitDelegate(DawnLibHandledEnemy);
        c.Emit(OpCodes.Brfalse_S, firstSkip);

        if (il.Method.Name.Contains("PredictAllOutsideEnemies") || il.Method.Name.Contains("SpawnRandomWeedEnemy"))
        {
            return;
        }

        int secondEnemyTypeLoc = -1;
        ILLabel secondSkip = null!;
        if (!c.TryGotoNext(
            MoveType.After,
            c => c.MatchLdfld<SpawnableEnemyWithRarity>(nameof(SpawnableEnemyWithRarity.enemyType)),
            c => c.MatchStloc(out secondEnemyTypeLoc),
            c => c.MatchLdarg(0),
            c => c.MatchLdfld(out _),
            c => c.MatchBrfalse(out secondSkip)
        ))
        {
            DawnPlugin.Logger.LogError($"Failed to apply {il.Method.Name} patch (2)!");
            return;
        }

        // emit function that takes in enemytype and return true or false, false to do vanilla behaviour
        c.Emit(OpCodes.Ldloc, secondEnemyTypeLoc);
        c.EmitDelegate(DawnLibHandledEnemy);
        c.Emit(OpCodes.Brfalse_S, secondSkip);
    }

    private static bool DawnLibHandledEnemy(EnemyType enemyType)
    {
        DawnEnemyInfo? enemyInfo = enemyType.DawnInfo;
        if (enemyInfo == null)
        {
            return true;
        }

        return enemyInfo.ShouldSkipRespectOverride();
    }

    private static void FixDawnEnemyReferences()
    {
        CadaverGrowthAI cadaverGrowthAI = LethalContent.Enemies[EnemyKeys.CadaverGrowths].EnemyType.enemyPrefab.GetComponent<CadaverGrowthAI>();
        GameObject FaceSporesPrefab = cadaverGrowthAI.faceSporesPrefab;
        GameObject CadaverSporesParticlePrefab = cadaverGrowthAI.CadaverSporesParticle;

        foreach (DawnEnemyInfo enemyInfo in LethalContent.Enemies.Values)
        {
            if (enemyInfo.ShouldSkipIgnoreOverride())
                continue;

            if (!enemyInfo.EnemyType.enemyPrefab.TryGetComponent(out CadaverGrowthAI growthAI))
            {
                continue;
            }

            growthAI.faceSporesPrefab = FaceSporesPrefab;
            growthAI.CadaverSporesParticle = CadaverSporesParticlePrefab;
        }
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
            if (enemyType.DawnInfo == null)
                continue;

            DawnEnemyInfo enemyInfo = enemyType.DawnInfo;
            if (enemyInfo.ShouldSkipRespectOverride())
                continue;

            if (enemyInfo.EnemyType.spawningDisabled)
                continue;

            if (enemyInfo.Inside.GetRarity() > 0)
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
            if (enemyInfo.ShouldSkipIgnoreOverride() || enemyInfo.BestiaryNode == null || enemyInfo.NameKeyword == null)
                continue;

            AddScanNodeToBestiaryEvent(enemyInfo.EnemyType.enemyPrefab, enemyInfo.BestiaryNode, enemyInfo.NameKeyword);
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
        itemInfoNouns.Add(new CompatibleNoun(nameKeyword, bestiaryNode));

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

            SpawnableEnemyWithRarity spawnDef = new(enemyInfo.EnemyType, 0);

            if (testLevel.Enemies.All(enemy => enemy.enemyType != enemyInfo.EnemyType))
            {
                Debuggers.Enemies?.Log($"Adding {enemyInfo.EnemyType} to test level {testLevel.name} inside.");
                testLevel.Enemies.Add(spawnDef);
            }

            if (testLevel.OutsideEnemies.All(enemy => enemy.enemyType != enemyInfo.EnemyType))
            {
                Debuggers.Enemies?.Log($"Adding {enemyInfo.EnemyType} to test level {testLevel.name} outside.");
                testLevel.OutsideEnemies.Add(spawnDef);
            }

            if (testLevel.DaytimeEnemies.All(enemy => enemy.enemyType != enemyInfo.EnemyType))
            {
                Debuggers.Enemies?.Log($"Adding {enemyInfo.EnemyType} to test level {testLevel.name} daytime.");
                testLevel.DaytimeEnemies.Add(spawnDef);
            }

            if (testLevel.OutsideEnemies.All(enemy => enemy.enemyType != enemyInfo.EnemyType))
            {
                Debuggers.Enemies?.Log($"Adding {enemyInfo.EnemyType} to test level {testLevel.name} outside (but it is a weed!).");
                testLevel.OutsideEnemies.Add(spawnDef);
            }
        }
        orig(self);
    }

    private static void EnsureCorrectEnemyVariables(On.EnemyAI.orig_Start orig, EnemyAI self)
    {
        if (self.enemyType.DawnInfo == null)
        {
            DawnPlugin.Logger.LogError($"Enemy with names {self.enemyType.name} and {self.enemyType.enemyName} has no DawnEnemyInfo, this means this enemy is not properly registered.");
            orig(self);
            return;
        }

        DawnEnemyInfo enemyInfo = self.enemyType.DawnInfo;
        if (enemyInfo.ShouldSkipRespectOverride() || StarlancerAIFixCompat.Enabled)
        {
            orig(self);
            return;
        }

        GameObject[]? insideNodes = RoundManager.Instance.insideAINodes;
        GameObject[]? outsideNodes = RoundManager.Instance.outsideAINodes;

        float closestDistance = float.MaxValue;
        bool insideIsClosest = true;
        if (insideNodes != null)
        {
            foreach (GameObject node in insideNodes)
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
            foreach (GameObject node in outsideNodes)
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

        DungeonFlow? dungeonFlow = RoundManagerRefs.GetCurrentDungeon();
        DawnMoonInfo moonInfo = level.DawnInfo;
        foreach (DawnEnemyInfo enemyInfo in LethalContent.Enemies.Values)
        {
            if (enemyInfo.ShouldSkipRespectOverride())
                continue;

            Debuggers.Enemies?.Log($"Updating weights for {enemyInfo.EnemyType} on level {level.PlanetName}");
            Debuggers.Enemies?.Log($"Updating Outside weights");
            SpawnableEnemyWithRarity? outsideSpawnableEnemyWithRarity = level.OutsideEnemies.FirstOrDefault(x => x.enemyType == enemyInfo.EnemyType);
            if (outsideSpawnableEnemyWithRarity == null)
            {
                outsideSpawnableEnemyWithRarity = new(enemyInfo.EnemyType, 0);
                level.OutsideEnemies.Add(outsideSpawnableEnemyWithRarity);
            }

            int outsideRarity = enemyInfo.Outside.GetRarity(moonInfo, dungeonFlow?.DawnInfo, level.currentWeather.DawnInfo);
            outsideSpawnableEnemyWithRarity.rarity = outsideRarity;

            Debuggers.Enemies?.Log($"Updating Inside weights");
            SpawnableEnemyWithRarity? insideSpawnableEnemyWithRarity = level.Enemies.FirstOrDefault(x => x.enemyType == enemyInfo.EnemyType);
            if (insideSpawnableEnemyWithRarity == null)
            {
                insideSpawnableEnemyWithRarity = new(enemyInfo.EnemyType, 0);
                level.Enemies.Add(insideSpawnableEnemyWithRarity);
            }

            int insideRarity = enemyInfo.Inside.GetRarity(moonInfo, dungeonFlow?.DawnInfo, level.currentWeather.DawnInfo);
            insideSpawnableEnemyWithRarity.rarity = insideRarity;

            Debuggers.Enemies?.Log($"Updating Daytime weights");
            SpawnableEnemyWithRarity? daytimeSpawnableEnemyWithRarity = level.DaytimeEnemies.FirstOrDefault(x => x.enemyType == enemyInfo.EnemyType);
            if (daytimeSpawnableEnemyWithRarity == null)
            {
                daytimeSpawnableEnemyWithRarity = new(enemyInfo.EnemyType, 0);
                level.DaytimeEnemies.Add(daytimeSpawnableEnemyWithRarity);
            }

            int daytimeRarity = enemyInfo.Daytime.GetRarity(moonInfo, dungeonFlow?.DawnInfo, level.currentWeather.DawnInfo);
            daytimeSpawnableEnemyWithRarity.rarity = daytimeRarity;

            Debuggers.Enemies?.Log($"Updating Weed weights");
            SpawnableEnemyWithRarity? weedSpawnableEnemyWithRarity = level.WeedEnemies.FirstOrDefault(x => x.enemyType == enemyInfo.EnemyType);
            if (weedSpawnableEnemyWithRarity == null)
            {
                Debuggers.Enemies?.Log($"Adding weed spawnable {enemyInfo.EnemyType} to the moon");
                weedSpawnableEnemyWithRarity = new(enemyInfo.EnemyType, 0);
                level.WeedEnemies.Add(weedSpawnableEnemyWithRarity);
            }

            int weedRarity = enemyInfo.Weed.GetRarity(moonInfo, dungeonFlow?.DawnInfo, level.currentWeather.DawnInfo);
            weedSpawnableEnemyWithRarity.rarity = weedRarity;
        }

        RoundManagerRefs.Instance.WeedEnemies.RemoveAll(x => !x.enemyType.DawnInfo.ShouldSkipRespectOverride());
        foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in level.WeedEnemies.ToList())
        {
            if (spawnableEnemyWithRarity.enemyType == null || spawnableEnemyWithRarity.enemyType.DawnInfo.ShouldSkipRespectOverride())
                continue;

            RoundManagerRefs.Instance.WeedEnemies.Add(spawnableEnemyWithRarity);
        }
    }

    private static void RegisterEnemies()
    {
        TerminalKeyword infoKeyword = TerminalRefs.InfoKeyword;
        foreach (EnemyType? enemyType in _networkPrefabEnemyTypes)
        {
            if (enemyType == null || enemyType.enemyPrefab == null)
                continue;

            if (enemyType.DawnInfo != null)
                continue;

            string name = NamespacedKey.NormalizeStringForNamespacedKey(enemyType.enemyName, true);
            NamespacedKey<DawnEnemyInfo>? key = EnemyKeys.GetByReflection(name);
            if (key == null && LethalLibCompat.Enabled && LethalLibCompat.TryGetEnemyTypeFromLethalLib(enemyType, out string lethalLibModName))
            {
                key = NamespacedKey<DawnEnemyInfo>.From(lethalLibModName, enemyType.enemyName);
            }
            else if (key == null && LethalLevelLoaderCompat.Enabled && LethalLevelLoaderCompat.TryGetExtendedEnemyTypeModName(enemyType, out string lethalLevelLoaderModName))
            {
                key = NamespacedKey<DawnEnemyInfo>.From(lethalLevelLoaderModName, enemyType.enemyName);
            }
            else if (key == null)
            {
                key = NamespacedKey<DawnEnemyInfo>.From("unknown_lib", enemyType.enemyName);
            }

            if (LethalContent.Enemies.ContainsKey(key))
            {
                DawnPlugin.Logger.LogWarning($"Enemy {enemyType.enemyName} is already registered by the same creator to LethalContent. This is likely to cause issues.");
                enemyType.DawnInfo = LethalContent.Enemies[key];
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
            DawnEnemyLocationInfo? weedInfo = null;

            WeightProfile<int> weedWeightProfile = new(DawnWeightChannels.EnemyRarity.Policy);
            weedWeightProfile.AddSource(new EnemyListBaseRaritySource(enemyType, level => RoundManagerRefs.Instance.WeedEnemies));
            DawnWeightedValue<int> weedRarity = new(DawnWeightChannels.EnemyRarity, weedWeightProfile);
            weedInfo = new DawnEnemyLocationInfo(weedRarity);

            WeightProfile<int> insideWeightProfile = new(DawnWeightChannels.EnemyRarity.Policy);
            insideWeightProfile.AddSource(new EnemyListBaseRaritySource(enemyType, level => level.Enemies));
            DawnWeightedValue<int> insideRarity = new(DawnWeightChannels.EnemyRarity, insideWeightProfile);
            insideInfo = new DawnEnemyLocationInfo(insideRarity);

            WeightProfile<int> outsideWeightProfile = new(DawnWeightChannels.EnemyRarity.Policy);
            outsideWeightProfile.AddSource(new EnemyListBaseRaritySource(enemyType, level => level.OutsideEnemies));
            DawnWeightedValue<int> outsideRarity = new(DawnWeightChannels.EnemyRarity, outsideWeightProfile);
            outsideInfo = new DawnEnemyLocationInfo(outsideRarity);

            WeightProfile<int> daytimeWeightProfile = new(DawnWeightChannels.EnemyRarity.Policy);
            daytimeWeightProfile.AddSource(new EnemyListBaseRaritySource(enemyType, level => level.DaytimeEnemies));
            DawnWeightedValue<int> daytimeRarity = new(DawnWeightChannels.EnemyRarity, daytimeWeightProfile);
            daytimeInfo = new DawnEnemyLocationInfo(daytimeRarity);

            HashSet<NamespacedKey> tags = [DawnLibTags.IsExternal];
            CollectLLLTags(enemyType, tags);

            TerminalNode? bestiaryNode = null;
            TerminalKeyword? nameKeyword = null;

            DawnEnemyInfo enemyInfo = new(
                key, tags, enemyType,
                outsideInfo, insideInfo, daytimeInfo, weedInfo,
                bestiaryNode, nameKeyword,
                null
            );
            enemyType.DawnInfo = enemyInfo;
            LethalContent.Enemies.Register(enemyInfo);
        }

        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            SelectableLevel level = moonInfo.Level;
            foreach (DawnEnemyInfo enemyInfo in LethalContent.Enemies.Values)
            {
                if (enemyInfo.ShouldSkipRespectOverride())
                    continue;

                TryAddToEnemyList(enemyInfo, level.OutsideEnemies);
                TryAddToEnemyList(enemyInfo, level.DaytimeEnemies);
                TryAddToEnemyList(enemyInfo, level.Enemies);
                TryAddToEnemyList(enemyInfo, level.WeedEnemies);
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

        SpawnableEnemyWithRarity spawnDef = new(enemyInfo.EnemyType, 0);
        list.Add(spawnDef);
    }
}