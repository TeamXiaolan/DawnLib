using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dawn.Internal;
using Dawn.Utils;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace Dawn;

[HarmonyPatch]
static class MoonRegistrationHandler
{
    public static MainGroupAlgorithm MoonGroupAlgorithm = new MainGroupAlgorithm();

    // See DuskPlugin
    internal static GameObject RouteProgressUIPrefab;

    internal static void Init()
    {
        LethalContent.Moons.AddAutoTaggers(
            new SimpleAutoTagger<DawnMoonInfo>(Tags.Company, moonInfo => !moonInfo.Level.spawnEnemiesAndScrap),
            new SimpleAutoTagger<DawnMoonInfo>(Tags.SupportsWeather, moonInfo => moonInfo.Level.spawnEnemiesAndScrap),
            new SimpleAutoTagger<DawnMoonInfo>(Tags.Free, moonInfo => moonInfo.RouteNode != null && moonInfo.RouteNode.itemCost == 0),
            new SimpleAutoTagger<DawnMoonInfo>(Tags.Paid, moonInfo => moonInfo.RouteNode != null && moonInfo.RouteNode.itemCost > 0),
            new SimpleAutoTagger<DawnMoonInfo>(DawnLibTags.HasBuyingPercent, moonInfo => moonInfo.GetNumberlessPlanetName() == "Gordion")
        );

        using (new DetourContext(priority: int.MaxValue - 10))
        {
            On.StartOfRound.Awake += CollectTestLevel;
            On.StartOfRound.Awake += CollectLevels;
            if (!DawnConfig.VanillaCompatibility.Value)
            {
                On.StartOfRound.Awake += SpawnRouteProgressUI;
                On.Terminal.Awake += RegisterDawnLevels;
            }
        }

        LethalContent.Moons.OnFreezeWithContext += _ => FixAmbienceLibraries();
        LethalContent.Enemies.OnFreezeWithContext += _ => FixDawnMoonEnemies();
        LethalContent.Items.OnFreezeWithContext += _ => FixDawnMoonItems();

        if (!DawnConfig.VanillaCompatibility.Value)
        {
            On.StartOfRound.ChangeLevel += StartOfRoundOnChangeLevel;
            On.StartOfRound.OnClientConnect += StartOfRoundOnClientConnect;
            On.StartOfRound.OnClientDisconnect += StartOfRoundOnClientDisconnect;

            On.StartOfRound.TravelToLevelEffects += DelayTravelEffects;

            On.Terminal.TextPostProcess += DynamicMoonCatalogue;

            IL.RoundManager.PredictAllOutsideEnemies += ReplaceStaticOutsideEnemyProbabilityRange;
        }

        if (!MoonDaySpeedMultiplierPatcherCompat.Enabled)
        {
            IL.TimeOfDay.MoveGlobalTime += MultiplyGlobalTimeMultiplierToDaySpeedMultiplier;
            IL.TimeOfDay.CalculatePlanetTime += IgnoreDaySpeedMultiplier;
            IL.TimeOfDay.Update += IgnoreDaySpeedMultiplier;
        }

        IL.RoundManager.SpawnRandomDaytimeEnemy += AccountForDaytimeDiversity;
        IL.RoundManager.SpawnRandomWeedEnemy += AccountForWeedDiversity;

        On.RoundManager.RefreshEnemiesList += UpdateNewerDiversity;
        On.RoundManager.UnloadSceneObjectsEarly += ResetNewerDiversity;

        On.RoundManager.RefreshEnemiesList += SetCurrentMaxPower;

        IL.RoundManager.SpawnDaytimeEnemiesOutside += ReplaceLevelValueWithRoundManager;
        IL.RoundManager.SpawnRandomDaytimeEnemy += ReplaceLevelValueWithRoundManager;

        IL.RoundManager.SpawnWeedEnemies += IntroduceMoreVariablesToSpawning;
    }

    private static void IntroduceMoreVariablesToSpawning(ILContext il)
    {
        ILCursor cursor = new(il);

        if (!cursor.TryGotoNext(
            MoveType.Before,
            il => il.MatchLdarg(0),
            il => il.MatchLdfld<RoundManager>(nameof(RoundManager.WeedEnemySpawnRandom)),
            il => il.MatchLdcI4(1),
            il => il.MatchLdcI4(3),
            il => il.MatchCallvirt<System.Random>(nameof(System.Random.Next))
        ))
        {
            DawnPlugin.Logger.LogWarning("Failed to apply RoundManager.SpawnWeedEnemies patch (1)");
            return;
        }

        cursor.Index++;
        cursor.RemoveRange(4);

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldfld, typeof(RoundManager).GetField(nameof(RoundManager.WeedEnemySpawnRandom)));

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(GetWeedEnemySpawnChanceThroughDay);

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(GetWeedSpawnProbabilityRange);

        cursor.EmitDelegate(DetermineNumberToSpawn);
    }

    private static int DetermineNumberToSpawn(RoundManager roundManager, System.Random weedEnemySpawnRandom, AnimationCurve weedEnemySpawnChanceThroughDay, float weedSpawnProbabilityRange)
    {
        float currentTime = roundManager.timeScript.lengthOfHours * roundManager.currentHour;
        float numberToSpawnAtCurrentTime = (int)(weedEnemySpawnChanceThroughDay.Evaluate(currentTime / roundManager.timeScript.totalTime) * 100f) / 100f;

        return weedEnemySpawnRandom.Next(Mathf.RoundToInt(numberToSpawnAtCurrentTime - weedSpawnProbabilityRange), Mathf.RoundToInt(numberToSpawnAtCurrentTime + weedSpawnProbabilityRange));
    }

    private static AnimationCurve GetWeedEnemySpawnChanceThroughDay(RoundManager roundManager)
    {
        return roundManager.currentLevel.WeedEnemySpawnChanceThroughDay;
    }

    private static float GetWeedSpawnProbabilityRange(RoundManager roundManager)
    {
        return roundManager.currentLevel.WeedEnemiesProbabilityRange;
    }

    private static void AccountForWeedDiversity(ILContext il)
    {
        ILCursor cursor = new(il);
        ILLabel zeroWeightLabel = null!;
        if (!cursor.TryGotoNext(
            MoveType.Before,
            il => il.MatchLdloc(1),
            il => il.MatchLdcI4(0),
            il => il.MatchStfld<EnemyType>(nameof(EnemyType.hasSpawnedAtLeastOne)),
            il => il.MatchLdloc(1),
            il => il.MatchLdfld<EnemyType>(nameof(EnemyType.PowerLevel)),
            il => il.MatchLdcR4(4),
            il => il.MatchLdarg(0),
            il => il.MatchLdfld<RoundManager>(nameof(RoundManager.currentWeedEnemyPower)),
            il => il.MatchSub(),
            il => il.MatchBgt(out zeroWeightLabel)))
        {
            DawnPlugin.Logger.LogWarning("Failed to apply RoundManager.SpawnRandomWeedEnemy patch (1)");
            return;
        }

        cursor.Emit(OpCodes.Ldloc, 1);

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(GetCurrentWeedMaxDiversity);

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(GetCurrentWeedDiversity);

        cursor.EmitDelegate(EnemyCanSpawnAccountingForDiversity);
        cursor.Emit(OpCodes.Brfalse_S, zeroWeightLabel);

        if (!cursor.TryGotoNext(
            MoveType.After,
            il => il.MatchLdloc(1),
            il => il.MatchLdcI4(0),
            il => il.MatchStfld<EnemyType>(nameof(EnemyType.hasSpawnedAtLeastOne)),
            il => il.MatchLdloc(1),
            il => il.MatchLdfld<EnemyType>(nameof(EnemyType.PowerLevel))))
        {
            DawnPlugin.Logger.LogWarning("Failed to apply RoundManager.SpawnRandomWeedEnemy patch (2)");
            return;
        }

        cursor.Remove(); // Replacing a LdcR4(4)
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(GetCurrentMaxWeedPower);

        if (!cursor.TryGotoNext(
            MoveType.Before,
            il => il.MatchLdarg(0),
            il => il.MatchLdfld<RoundManager>(nameof(RoundManager.increasedOutsideEnemySpawnRateIndex)),
            il => il.MatchLdloc(7),
            il => il.MatchBneUn(out _),
            il => il.MatchLdcI4(100),
            il => il.MatchStloc(8),
            il => il.MatchBr(out _)
        ))
        {
            DawnPlugin.Logger.LogWarning("Failed to apply RoundManager.SpawnRandomWeedEnemy patch (3)");
            return;
        }

        cursor.MoveAfterLabels();
        cursor.RemoveRange(7);

        if (!cursor.TryGotoNext(
            MoveType.After,
            il => il.MatchLdarg(0),
            il => il.MatchLdfld<RoundManager>(nameof(RoundManager.outsideAINodes)),
            il => il.MatchStloc(6),
            il => il.MatchLdcI4(0),
            il => il.MatchStloc(9),
            il => il.MatchBr(out _),
            il => il.MatchLdloc(4),
            il => il.MatchLdfld<EnemyType>(nameof(EnemyType.PowerLevel))
        ))
        {
            DawnPlugin.Logger.LogWarning("Failed to apply RoundManager.SpawnRandomWeedEnemy patch (4)");
            return;
        }

        cursor.Remove(); // Replacing a LdcR4(4)
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(GetCurrentMaxWeedPower);

        if (!cursor.TryGotoNext(
            MoveType.Before,
            il => il.MatchLdloc(11),
            il => il.MatchCallvirt(out _),
            il => il.MatchLdfld<EnemyAI>(nameof(EnemyAI.enemyType)),
            il => il.MatchDup(),
            il => il.MatchLdfld<EnemyType>(nameof(EnemyType.numberSpawned)),
            il => il.MatchLdcI4(1),
            il => il.MatchAdd(),
            il => il.MatchStfld<EnemyType>(nameof(EnemyType.numberSpawned)),
            il => il.MatchLdloc(11),
            il => il.MatchCallvirt(out _)
        ))
        {
            DawnPlugin.Logger.LogWarning("Failed to apply RoundManager.SpawnRandomWeedEnemy patch (5)");
            return;
        }

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldloc, 4);
        cursor.EmitDelegate(IncrementWeedDiversity);
    }

    private static void IncrementWeedDiversity(RoundManager roundManager, EnemyType enemyType)
    {
        if (enemyType.hasSpawnedAtLeastOne)
        {
            return;
        }

        roundManager.CurrentWeedEnemyDiversityLevel += enemyType.DiversityPowerLevel;
    }

    private static int GetCurrentWeedDiversity(RoundManager roundManager)
    {
        return roundManager.CurrentWeedEnemyDiversityLevel;
    }

    private static int GetCurrentWeedMaxDiversity(RoundManager roundManager)
    {
        return roundManager.CurrentMaxWeedDiversityLevel;
    }

    private static float GetCurrentMaxWeedPower(RoundManager roundManager)
    {
        return roundManager.CurrentMaxWeedPower;
    }

    private static void ReplaceLevelValueWithRoundManager(ILContext il)
    {
        ILCursor cursor = new(il);

        // evil for loop.
        for (; cursor.Index < cursor.Instrs.Count; cursor.Index++)
        {
            if (cursor.Next.OpCode != OpCodes.Ldfld && cursor.Next.Next != null && cursor.Next.Next.OpCode != OpCodes.Ldfld)
                continue;

            if (cursor.Next.MatchLdfld<RoundManager>(nameof(RoundManager.currentLevel)) && cursor.Next.Next.MatchLdfld<SelectableLevel>(nameof(SelectableLevel.maxDaytimeEnemyPowerCount)))
            {
                cursor.Index += 2;
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate((int _, RoundManager self) =>
                {
                    return self.CurrentMaxDaytimePower;
                });
                continue;
            }
        }
    }

    private static void SetCurrentMaxPower(On.RoundManager.orig_RefreshEnemiesList orig, RoundManager self)
    {
        orig(self);

        self.CurrentMaxDaytimePower = self.currentLevel.maxDaytimeEnemyPowerCount;

        self.CurrentMaxWeedPower = self.currentLevel.MaxWeedEnemyPowerCount;
    }

    private static void ResetNewerDiversity(On.RoundManager.orig_UnloadSceneObjectsEarly orig, RoundManager self)
    {
        orig(self);

        self.CurrentMaxDaytimeDiversityLevel = 0;
        self.CurrentDaytimeEnemyDiversityLevel = 0;

        self.CurrentMaxWeedDiversityLevel = 0;
        self.CurrentWeedEnemyDiversityLevel = 0;
    }

    private static void UpdateNewerDiversity(On.RoundManager.orig_RefreshEnemiesList orig, RoundManager self)
    {
        self.CurrentMaxDaytimeDiversityLevel = self.currentLevel.MaxDaytimeDiversityPowerCount;

        self.CurrentMaxWeedDiversityLevel = self.currentLevel.MaxWeedDiversityPowerCount;

        orig(self);
    }

    private static void AccountForDaytimeDiversity(ILContext il)
    {
        ILCursor cursor = new(il);
        ILLabel firstSkipLabel = null!;
        if (!cursor.TryGotoNext(
            MoveType.After,
            il => il.MatchLdfld<EnemyType>(nameof(EnemyType.normalizedTimeInDayToLeave)),
            il => il.MatchCall<TimeOfDay>("get_Instance"),
            il => il.MatchLdfld<TimeOfDay>(nameof(TimeOfDay.normalizedTimeOfDay)),
            il => il.MatchBlt(out firstSkipLabel),
            il => il.MatchLdloc(2),
            il => il.MatchLdfld<EnemyType>(nameof(EnemyType.spawningDisabled)),
            il => il.MatchBrtrue(out _)))
        {
            DawnPlugin.Logger.LogWarning("Failed to apply RoundManager.SpawnRandomDaytimeEnemy patch (1)");
            return;
        }

        cursor.Emit(OpCodes.Ldloc, 2);

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(GetCurrentDaytimeMaxDiversity);

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(GetCurrentDaytimeDiversity);

        cursor.EmitDelegate(EnemyCanSpawnAccountingForDiversity);
        cursor.Emit(OpCodes.Brfalse_S, firstSkipLabel);

        ILLabel zeroWeightLabel = null!;
        if (!cursor.TryGotoNext(
            MoveType.Before,
            il => il.MatchLdloc(2),
            il => il.MatchLdfld<EnemyType>(nameof(EnemyType.PowerLevel)),
            il => il.MatchLdarg(0),
            il => il.MatchLdfld<RoundManager>(nameof(RoundManager.currentLevel)),
            il => il.MatchLdfld<SelectableLevel>(nameof(SelectableLevel.maxDaytimeEnemyPowerCount)),
            il => il.MatchConvR4(),
            il => il.MatchLdloc(0),
            il => il.MatchSub(),
            il => il.MatchBgt(out zeroWeightLabel)
        ))
        {
            DawnPlugin.Logger.LogWarning("Failed to apply RoundManager.SpawnRandomDaytimeEnemy patch (2)");
            return;
        }

        cursor.Emit(OpCodes.Ldloc, 2);

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(GetCurrentDaytimeMaxDiversity);

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(GetCurrentDaytimeDiversity);

        cursor.EmitDelegate(EnemyCanSpawnAccountingForDiversity);
        cursor.Emit(OpCodes.Brfalse_S, zeroWeightLabel);

        if (!cursor.TryGotoNext(
            MoveType.Before,
            il => il.MatchLdloc(14),
            il => il.MatchCallvirt(out _),
            il => il.MatchLdfld<EnemyAI>(nameof(EnemyAI.enemyType)),
            il => il.MatchDup(),
            il => il.MatchLdfld<EnemyType>(nameof(EnemyType.numberSpawned)),
            il => il.MatchLdcI4(1),
            il => il.MatchAdd(),
            il => il.MatchStfld<EnemyType>(nameof(EnemyType.numberSpawned)),
            il => il.MatchLdloc(14),
            il => il.MatchCallvirt(out _)
        ))
        {
            DawnPlugin.Logger.LogWarning("Failed to apply RoundManager.SpawnRandomDaytimeEnemy patch (3)");
            return;
        }

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldloc, 5);
        cursor.EmitDelegate(IncrementDaytimeDiversity);
    }

    private static void IncrementDaytimeDiversity(RoundManager roundManager, EnemyType enemyType)
    {
        if (enemyType.hasSpawnedAtLeastOne)
        {
            return;
        }

        roundManager.CurrentDaytimeEnemyDiversityLevel += enemyType.DiversityPowerLevel;
    }

    private static bool EnemyCanSpawnAccountingForDiversity(EnemyType enemyType, int currentMaxDiversityLevel, int currentEnemyDiversityLevel)
    {
        return enemyType.DiversityPowerLevel <= currentMaxDiversityLevel - currentEnemyDiversityLevel || enemyType.hasSpawnedAtLeastOne;
    }

    private static int GetCurrentDaytimeDiversity(RoundManager roundManager)
    {
        return roundManager.CurrentDaytimeEnemyDiversityLevel;
    }

    private static int GetCurrentDaytimeMaxDiversity(RoundManager roundManager)
    {
        return roundManager.CurrentMaxDaytimeDiversityLevel;
    }

    private static void ReplaceStaticOutsideEnemyProbabilityRange(ILContext il)
    {
        ILCursor cursor = new(il);
        if (!cursor.TryGotoNext(MoveType.After,
            i => i.MatchLdloc(out _),
            i => i.MatchLdloc(out _),
            i => i.MatchLdcR4(3f)))
        {
            DawnPlugin.Logger.LogWarning("Failed to apply RoundManager.PredictAllOutsideEnemies patch (1)");
            return;
        }

        cursor.Emit(OpCodes.Pop);
        cursor.Emit(OpCodes.Call, typeof(MoonRegistrationHandler).GetMethod(nameof(GetMoonOutsideEnemyProbabilitySpawnRange), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static));

        if (!cursor.TryGotoNext(MoveType.After,
            il => il.MatchConvI4(),
            il => il.MatchLdloc(out _),
            il => il.MatchLdcR4(3f)))
        {
            DawnPlugin.Logger.LogWarning("Failed to apply RoundManager.PredictAllOutsideEnemies patch (2)");
            return;
        }

        cursor.Emit(OpCodes.Pop);
        cursor.Emit(OpCodes.Call, typeof(MoonRegistrationHandler).GetMethod(nameof(GetMoonOutsideEnemyProbabilitySpawnRange), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static));
    }

    private static float GetMoonOutsideEnemyProbabilitySpawnRange()
    {
        if (StartOfRound.Instance == null || StartOfRound.Instance.currentLevel == null || StartOfRound.Instance.currentLevel.DawnInfo == null)
        {
            return 3f;
        }

        return StartOfRound.Instance.currentLevel.OutsideEnemiesProbabilityRange;
    }

    private static void MultiplyGlobalTimeMultiplierToDaySpeedMultiplier(ILContext il)
    {
        ILCursor cursor = new(il);
        if (!cursor.TryGotoNext(MoveType.After,
            il => il.MatchLdarg(0),
            il => il.MatchLdfld<TimeOfDay>("globalTimeSpeedMultiplier")))
        {
            DawnPlugin.Logger.LogWarning("Failed to apply TimeOfDay.MoveGlobalTime patch");
            return;
        }

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldfld, typeof(TimeOfDay).GetField(nameof(TimeOfDay.currentLevel)));
        cursor.Emit(OpCodes.Ldfld, typeof(SelectableLevel).GetField(nameof(SelectableLevel.DaySpeedMultiplier)));
        cursor.Emit(OpCodes.Mul);
    }

    private static void IgnoreDaySpeedMultiplier(ILContext il)
    {
        ILCursor cursor = new(il);
        if (!cursor.TryGotoNext(MoveType.After,
            il => il.MatchLdfld<SelectableLevel>("DaySpeedMultiplier")))
        {
            DawnPlugin.Logger.LogWarning("Failed to apply TimeOfDay.Update patch");
            return;
        }

        cursor.Emit(OpCodes.Pop);
        cursor.Emit(OpCodes.Ldc_R4, 1f);
    }

    private static void SpawnRouteProgressUI(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        UnityEngine.Object.Instantiate(RouteProgressUIPrefab, self.radarCanvas.transform);
        orig(self);
    }

    private static void FixAmbienceLibraries()
    {
        List<LevelAmbienceLibrary> vanillaLevelAmbienceLibraries = new();
        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            if (!moonInfo.TypedKey.IsVanilla())
                continue;

            if (moonInfo.Level.levelAmbienceClips != null)
            {
                vanillaLevelAmbienceLibraries.Add(moonInfo.Level.levelAmbienceClips);
            }
            vanillaLevelAmbienceLibraries.AddRange(moonInfo.Level.dungeonFlowTypes.Select(dungeonFlowType => dungeonFlowType.overrideLevelAmbience).Where(x => x != null));
        }
        vanillaLevelAmbienceLibraries = vanillaLevelAmbienceLibraries.Distinct().ToList();

        List<LevelAmbienceLibrary> ambiencesToDestroy = new();
        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            if (moonInfo.ShouldSkipIgnoreOverride())
                continue;

            foreach (LevelAmbienceLibrary levelAmbienceLibrary in vanillaLevelAmbienceLibraries)
            {
                if (moonInfo.Level.levelAmbienceClips != null && moonInfo.Level.levelAmbienceClips.name == levelAmbienceLibrary.name)
                {
                    ambiencesToDestroy.Add(moonInfo.Level.levelAmbienceClips);
                    moonInfo.Level.levelAmbienceClips = levelAmbienceLibrary;
                }

                for (int i = 0; i < moonInfo.Level.dungeonFlowTypes.Length; i++)
                {
                    LevelAmbienceLibrary? overrideLevelAmbienceLibrary = moonInfo.Level.dungeonFlowTypes[i].overrideLevelAmbience;
                    if (overrideLevelAmbienceLibrary == null)
                        continue;

                    if (overrideLevelAmbienceLibrary.name != levelAmbienceLibrary.name)
                        continue;

                    ambiencesToDestroy.Add(overrideLevelAmbienceLibrary);
                    moonInfo.Level.dungeonFlowTypes[i].overrideLevelAmbience = levelAmbienceLibrary;
                }
            }
        }

        ambiencesToDestroy = ambiencesToDestroy.Where(x => !vanillaLevelAmbienceLibraries.Contains(x)).Distinct().ToList();
        for (int i = ambiencesToDestroy.Count - 1; i >= 0; i--)
        {
            ScriptableObject.Destroy(ambiencesToDestroy[i]);
        }
    }

    private static void RegisterDawnLevels(On.Terminal.orig_Awake orig, Terminal self)
    {
        List<SelectableLevel> levels = StartOfRoundRefs.Instance.levels.ToList();
        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            if (moonInfo.ShouldSkipIgnoreOverride())
                continue;

            moonInfo.Level.levelID = levels.Count;
            levels.Add(moonInfo.Level);
            UpdateMoonPrice(moonInfo);
        }
        StartOfRoundRefs.Instance.levels = levels.ToArray();

        if (LethalContent.Moons.IsFrozen)
        {
            orig(self);
            return;
        }

        List<TerminalKeyword> allKeywords = TerminalRefs.Instance.terminalNodes.allKeywords.ToList();
        List<CompatibleNoun> routeNouns = TerminalRefs.RouteKeyword.compatibleNouns.ToList();
        List<SelectableLevel> viewableLevels = TerminalRefs.Instance.moonsCatalogueList.ToList();
        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            if (moonInfo.ShouldSkipIgnoreOverride())
                continue;

            if (moonInfo.ReceiptNode == null || moonInfo.RouteNode == null || moonInfo.NameKeyword == null)
                continue;

            moonInfo.ReceiptNode.buyRerouteToMoon = moonInfo.Level.levelID;
            moonInfo.RouteNode.displayPlanetInfo = moonInfo.Level.levelID;

            routeNouns.Add(new CompatibleNoun(moonInfo.NameKeyword, moonInfo.RouteNode));
            allKeywords.Add(moonInfo.NameKeyword);
            moonInfo.NameKeyword.defaultVerb = TerminalRefs.RouteKeyword;

            moonInfo.RouteNode.overrideOptions = true;
            moonInfo.RouteNode.terminalOptions = [
                new CompatibleNoun(TerminalRefs.DenyKeyword, TerminalRefs.CancelRouteNode),
                new CompatibleNoun(TerminalRefs.ConfirmPurchaseKeyword, moonInfo.ReceiptNode)
            ];

            if (moonInfo.DawnPurchaseInfo.PurchasePredicate.CanPurchase() is not TerminalPurchaseResult.HiddenPurchaseResult)
            {
                viewableLevels.Add(moonInfo.Level);
            }
        }
        TerminalRefs.Instance.moonsCatalogueList = viewableLevels.ToArray();
        TerminalRefs.RouteKeyword.compatibleNouns = routeNouns.ToArray();
        TerminalRefs.Instance.terminalNodes.allKeywords = allKeywords.ToArray();
        orig(self);
    }

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Start)), HarmonyPrefix, HarmonyPriority(-999)]
    private static void FreezeMoonRegistry()
    {
        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            if (moonInfo.Level.indoorMapHazards == null)
            {
                moonInfo.Level.indoorMapHazards = [];
            }
        }

        if (LethalContent.Moons.IsFrozen)
        {
            return;
        }

        LethalContent.Moons.Freeze();
    }

    private static void FixDawnMoonItems()
    {
        List<Item> itemsToDestroy = new();

        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            if (moonInfo.ShouldSkipIgnoreOverride())
                continue;

            foreach (SpawnableItemWithRarity spawnableItemWithRarity in moonInfo.Level.spawnableScrap.ToArray())
            {
                if (spawnableItemWithRarity.spawnableItem == null)
                {
                    moonInfo.Level.spawnableScrap.Remove(spawnableItemWithRarity);
                    continue;
                }

                bool itemIsValid = spawnableItemWithRarity.spawnableItem.DawnInfo != null;
                foreach (DawnItemInfo itemInfo in LethalContent.Items.Values)
                {
                    if (!itemIsValid && itemInfo.Item.name == spawnableItemWithRarity.spawnableItem.name)
                    {
                        itemsToDestroy.Add(spawnableItemWithRarity.spawnableItem);
                        spawnableItemWithRarity.spawnableItem = itemInfo.Item;
                        break;
                    }
                }
            }
        }

        for (int i = itemsToDestroy.Count - 1; i >= 0; i--)
        {
            Item.Destroy(itemsToDestroy[i]);
        }
    }

    private static void FixDawnMoonEnemies()
    {
        List<EnemyType> enemiesToDestroy = new();

        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            if (moonInfo.ShouldSkipIgnoreOverride())
                continue;

            EnemyType? specialEnemy = moonInfo.Level.specialEnemyRarity?.overrideEnemy;
            foreach (DawnEnemyInfo enemyInfo in LethalContent.Enemies.Values)
            {
                EnemyType potentialReplacement = enemyInfo.EnemyType;
                ReplaceAndSetToDestroy(moonInfo.Level.Enemies, potentialReplacement, enemiesToDestroy);
                ReplaceAndSetToDestroy(moonInfo.Level.OutsideEnemies, potentialReplacement, enemiesToDestroy);
                ReplaceAndSetToDestroy(moonInfo.Level.DaytimeEnemies, potentialReplacement, enemiesToDestroy);
                ReplaceAndSetToDestroy(moonInfo.Level.WeedEnemies, potentialReplacement, enemiesToDestroy);

                if (specialEnemy != null)
                {
                    bool enemyIsValid = specialEnemy.DawnInfo != null;
                    if (enemyIsValid)
                    {
                        continue;
                    }

                    if (potentialReplacement.name == specialEnemy.name)
                    {
                        Debuggers.Moons?.Log($"replacing fake SO {specialEnemy.name} with {potentialReplacement.name}");
                        enemiesToDestroy.Add(specialEnemy);
                        specialEnemy = potentialReplacement;
                    }
                }
            }
        }


        for (int i = enemiesToDestroy.Count - 1; i >= 0; i--)
        {
            if (enemiesToDestroy[i] == null)
            {
                continue;
            }

            ScriptableObject.Destroy(enemiesToDestroy[i]);
        }
    }

    private static void ReplaceAndSetToDestroy(List<SpawnableEnemyWithRarity> spawnableEnemiesWithRarities, EnemyType potentialReplacement, List<EnemyType> enemiesToDestroy)
    {
        for (int i = spawnableEnemiesWithRarities.Count - 1; i >= 0; i--)
        {
            SpawnableEnemyWithRarity spawnableEnemyWithRarity = spawnableEnemiesWithRarities[i];
            if (spawnableEnemyWithRarity.enemyType == null)
            {
                spawnableEnemiesWithRarities.RemoveAt(i);
                continue;
            }

            bool enemyIsValid = spawnableEnemyWithRarity.enemyType.DawnInfo != null;
            if (enemyIsValid)
            {
                continue;
            }

            if (potentialReplacement.name == spawnableEnemyWithRarity.enemyType.name)
            {
                Debuggers.Moons?.Log($"replacing fake SO {spawnableEnemyWithRarity.enemyType.name} with {potentialReplacement.name}");
                enemiesToDestroy.Add(spawnableEnemyWithRarity.enemyType);
                spawnableEnemyWithRarity.enemyType = potentialReplacement;
                break;
            }

        }
    }

    // I think I wrote this nicely, I hope you're proud of me Bongo
    private static string DynamicMoonCatalogue(On.Terminal.orig_TextPostProcess orig, Terminal self, string modifieddisplaytext, TerminalNode node)
    {
        if (node != TerminalRefs.MoonCatalogueNode)
        {
            return orig(self, modifieddisplaytext, node);
        }

        StringBuilder builder = new StringBuilder("\n\nWelcome to the exomoons catalogue.\nTo route the autopilot to a moon, use the word ROUTE.\nTo learn about any moon, use INFO.\n____________________________\n");
        IEnumerable<DawnMoonInfo> validMoons = LethalContent.Moons.Values
            .Where(it => !it.HasTag(Tags.Unimplemented))
            .OrderByDescending(it => it.HasTag(Tags.Vanilla));

        List<MoonGroup> groups = MoonGroupAlgorithm.Group(validMoons);

        foreach (MoonGroup group in groups)
        {
            builder.AppendLine("");

            if (!string.IsNullOrWhiteSpace(group.GroupName))
            {
                builder.AppendLine(group.GroupName);
            }

            foreach (DawnMoonInfo moonInfo in group.Moons)
            {
                TerminalPurchaseResult result = moonInfo.DawnPurchaseInfo.PurchasePredicate.CanPurchase();
                builder.AppendLine(FormatMoonEntry(moonInfo, result));
            }
        }

        return orig(self, builder.ToString(), node);
    }

    public static string FormatMoonEntry(DawnMoonInfo moonInfo, TerminalPurchaseResult result)
    {
        StringBuilder builder = new StringBuilder();
        string name = moonInfo.GetNumberlessPlanetName();
        if (result is TerminalPurchaseResult.FailedPurchaseResult failedResult)
        {
            name = failedResult.OverrideName ?? name;
        }

        if (name == "Gordion")
        {
            name = "The Company building";
        }

        builder.Append($"* {name} ");

        if (moonInfo.HasTag(DawnLibTags.HasBuyingPercent))
        {
            builder.Append("//  Buying at [companyBuyingPercent].");
        }
        else
        {
            TryAppendMoonWeather(builder, moonInfo);
        }

        return builder.ToString();
    }

    public static void TryAppendMoonWeather(StringBuilder builder, DawnMoonInfo moonInfo)
    {
        DawnWeatherEffectInfo? currentWeather = moonInfo.GetCurrentWeather();

        if (currentWeather != null)
        {
            builder.Append($"({moonInfo.Level.currentWeather})");
        }
    }

    private static IEnumerator DelayTravelEffects(On.StartOfRound.orig_TravelToLevelEffects orig, StartOfRound self)
    {
        IEnumerator enumerator = orig(self);
        while (enumerator.MoveNext())
        {
            yield return enumerator.Current;
            if (enumerator.Current is WaitForSeconds wfs && Mathf.Approximately(wfs.m_Seconds, self.currentLevel.timeToArrive) && DawnMoonNetworker.IsNotNull)
            {
                yield return new WaitUntil(() => DawnMoonNetworker.Instance.allPlayersDone);
            }
        }
        self.shipTravelCoroutine = null;
    }

    private static void StartOfRoundOnClientDisconnect(On.StartOfRound.orig_OnClientDisconnect orig, StartOfRound self, ulong clientid)
    {
        orig(self, clientid);

        if (self.IsServer && self.inShipPhase)
        {
            DawnMoonNetworker.Instance?.HostRebroadcastQueue();
        }
    }

    private static void StartOfRoundOnClientConnect(On.StartOfRound.orig_OnClientConnect orig, StartOfRound self, ulong clientid)
    {
        orig(self, clientid);

        if (self.IsServer && self.inShipPhase)
        {
            DawnMoonNetworker.Instance?.HostRebroadcastQueue();
        }
    }

    private static void StartOfRoundOnChangeLevel(On.StartOfRound.orig_ChangeLevel orig, StartOfRound self, int levelid)
    {
        orig(self, levelid);

        if (self.IsServer)
        {
            self.StartCoroutine(DoHotloadSceneStuff(self.currentLevel));
        }
    }

    static IEnumerator DoHotloadSceneStuff(SelectableLevel level)
    {
        yield return new WaitUntil(() => DawnMoonNetworker.IsNotNull && RouteProgressUI.IsNotNull);
        DawnMoonNetworker.Instance!.HostDecide(level.DawnInfo);
    }

    private static void CollectLevels(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        orig(self);
        if (LethalContent.Moons.IsFrozen)
        {
            return;
        }

        _ = TerminalRefs.Instance;
        foreach (SelectableLevel level in StartOfRound.Instance.levels)
        {
            if (level.DawnInfo != null)
                continue;

            Debuggers.Moons?.Log($"Registering level: {level.PlanetName} with scrap spawn range of: {level.minScrap} and {level.maxScrap}");
            NamespacedKey<DawnMoonInfo>? key = MoonKeys.GetByReflection(NamespacedKey.NormalizeStringForNamespacedKey(level.PlanetName, true).RemoveEnd("Level"));
            if (key == null && LethalLevelLoaderCompat.Enabled && LethalLevelLoaderCompat.TryGetExtendedLevelModName(level, out string moonModName))
            {
                key = NamespacedKey<DawnMoonInfo>.From(moonModName, level.PlanetName);
            }
            else if (key == null)
            {
                key = NamespacedKey<DawnMoonInfo>.From("unknown_modded", level.PlanetName);
            }

            HashSet<NamespacedKey> tags = [DawnLibTags.IsExternal];
            CollectLLLTags(level, tags);

            TerminalNode? routeNode = null;
            TerminalNode? receiptNode = null;
            TerminalKeyword? nameKeyword = null;
            foreach (CompatibleNoun compatibleNoun in TerminalRefs.RouteKeyword.compatibleNouns)
            {
                if (compatibleNoun.result.displayPlanetInfo == level.levelID)
                {
                    routeNode = compatibleNoun.result;
                    if (routeNode.terminalOptions.Length > 1) receiptNode = routeNode.terminalOptions[1].result;
                    nameKeyword = compatibleNoun.noun;
                    break;
                }
            }

            ITerminalPurchasePredicate predicate = ITerminalPurchasePredicate.AlwaysSuccess();
            if (LethalLevelLoaderCompat.Enabled && LethalLevelLoaderCompat.ExtendedLevelIsModded(level, out object? extendedLevel))
            {
                predicate = new LethalLevelLoaderTerminalPredicate(extendedLevel);
            }
            else if (LethalLevelLoaderCompat.Enabled && DawnConfig.AllowLLLToOverrideVanillaStatus.Value && key.Namespace == NamespacedKey.VanillaNamespace)
            {
                predicate = new LethalLevelLoaderTerminalPredicate(level);
            }
            else if (Equals(key, MoonKeys.Embrion) || Equals(key, MoonKeys.Artifice))
            {
                predicate = new ConstantTerminalPredicate(TerminalPurchaseResult.Hidden().SetFailure(false));
            }

            DawnMoonInfo moonInfo = new DawnMoonInfo(key, tags, level, 3f, 100, 4, 100, RoundManagerRefs.Instance.WeedEnemies.ToList(), AnimationCurve.Constant(0f, 1f, 2f), 1f, new([new VanillaMoonSceneInfo(key.AsTyped<IMoonSceneInfo>(), level.sceneName)]), routeNode, receiptNode, nameKeyword, new DawnPurchaseInfo(new SimpleProvider<int>(routeNode?.itemCost ?? -1), predicate), null);
            level.DawnInfo = moonInfo;
            LethalContent.Moons.Register(moonInfo);
        }
    }

    private static void CollectTestLevel(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        if (LethalContent.Moons.IsFrozen)
        {
            orig(self);
            return;
        }

        DawnMoonInfo testMoonInfo = new(MoonKeys.Test, [DawnLibTags.IsExternal], self.currentLevel, 3f, 100, 4, 100, RoundManagerRefs.Instance.WeedEnemies.ToList(), AnimationCurve.Constant(0f, 1f, 2f), 1f, new(), null, null, null, new DawnPurchaseInfo(new SimpleProvider<int>(-1), ITerminalPurchasePredicate.AlwaysHide()), null);
        self.currentLevel.DawnInfo = testMoonInfo;
        LethalContent.Moons.Register(testMoonInfo);
        orig(self);
    }

    private static void CollectLLLTags(SelectableLevel moon, HashSet<NamespacedKey> tags)
    {
        if (LethalLevelLoaderCompat.Enabled && LethalLevelLoaderCompat.TryGetAllTagsWithModNames(moon, out List<(string modName, string tagName)> tagsWithModNames))
        {
            tags.AddToList(tagsWithModNames, Debuggers.Moons, moon.name);
        }
    }

    internal static void UpdateAllPrices()
    {
        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            if (moonInfo.ShouldSkipRespectOverride())
                continue;

            UpdateMoonPrice(moonInfo);
        }
    }

    private static void UpdateMoonPrice(DawnMoonInfo moonInfo)
    {
        int cost = moonInfo.DawnPurchaseInfo.Cost.Provide();
        if (moonInfo.RouteNode != null)
        {
            moonInfo.RouteNode.itemCost = cost;
        }

        if (moonInfo.ReceiptNode != null)
        {
            moonInfo.ReceiptNode.itemCost = cost;
        }
    }
}