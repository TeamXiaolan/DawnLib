using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dawn.Internal;
using Dawn.Utils;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace Dawn;

static class MoonRegistrationHandler
{
    private static IMoonGroupAlgorithm _groupAlgorithm = new RankGroupAlgorithm();

    internal static void Init()
    {
        LethalContent.Moons.AddAutoTaggers(
            new SimpleAutoTagger<DawnMoonInfo>(Tags.Company, moonInfo => !moonInfo.Level.spawnEnemiesAndScrap),
            new SimpleAutoTagger<DawnMoonInfo>(Tags.Free, moonInfo => moonInfo.RouteNode && moonInfo.RouteNode!.itemCost == 0),
            new SimpleAutoTagger<DawnMoonInfo>(Tags.Paid, moonInfo => moonInfo.RouteNode && moonInfo.RouteNode!.itemCost > 0),
            new SimpleAutoTagger<DawnMoonInfo>(DawnLibTags.HasBuyingPercent, moonInfo => moonInfo.GetNumberlessPlanetName() == "Gordion")
        );

        using (new DetourContext(priority: int.MaxValue - 10))
        {
            On.StartOfRound.Awake += CollectTestLevel;
            On.StartOfRound.Awake += CollectLevels;
            On.Terminal.Awake += RegisterDawnLevels;
        }

        LethalContent.Moons.OnFreeze += FixAmbienceLibraries;
        LethalContent.Enemies.OnFreeze += FixDawnMoonEnemies;
        LethalContent.Items.OnFreeze += FixDawnMoonItems;

        On.StartOfRound.ChangeLevel += StartOfRoundOnChangeLevel;
        On.StartOfRound.OnClientConnect += StartOfRoundOnClientConnect;
        On.StartOfRound.OnClientDisconnect += StartOfRoundOnClientDisconnect;

        On.StartOfRound.TravelToLevelEffects += DelayTravelEffects;

        On.Terminal.TextPostProcess += DynamicMoonCatalogue;
    }

    private static void FixAmbienceLibraries()
    {
        List<LevelAmbienceLibrary> vanillaLevelAmbienceLibraries = new();
        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            if (!moonInfo.TypedKey.IsVanilla())
                continue;

            if (moonInfo.Level.levelAmbienceClips != null) vanillaLevelAmbienceLibraries.Add(moonInfo.Level.levelAmbienceClips);
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
        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            if (moonInfo.ShouldSkipIgnoreOverride())
                continue;

            if (moonInfo.ReceiptNode == null || moonInfo.RouteNode == null || moonInfo.NameKeyword == null)
                continue;

            moonInfo.ReceiptNode.buyRerouteToMoon = moonInfo.Level.levelID;
            moonInfo.RouteNode.displayPlanetInfo = moonInfo.Level.levelID;

            routeNouns.Add(new CompatibleNoun()
            {
                noun = moonInfo.NameKeyword,
                result = moonInfo.RouteNode
            });
            allKeywords.Add(moonInfo.NameKeyword);
            moonInfo.NameKeyword.defaultVerb = TerminalRefs.RouteKeyword;

            moonInfo.RouteNode.overrideOptions = true;
            moonInfo.RouteNode.terminalOptions = [
                new CompatibleNoun()
                {
                    noun = TerminalRefs.DenyKeyword,
                    result = TerminalRefs.CancelRouteNode
                },
                new CompatibleNoun()
                {
                    noun = TerminalRefs.ConfirmPurchaseKeyword,
                    result = moonInfo.ReceiptNode
                }
            ];
        }
        TerminalRefs.RouteKeyword.compatibleNouns = routeNouns.ToArray();
        TerminalRefs.Instance.terminalNodes.allKeywords = allKeywords.ToArray();
        LethalContent.Moons.Freeze();
        orig(self);
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

                bool enemyIsValid = spawnableItemWithRarity.spawnableItem.HasDawnInfo();
                foreach (DawnItemInfo itemInfo in LethalContent.Items.Values)
                {
                    if (!enemyIsValid && itemInfo.Item.name == spawnableItemWithRarity.spawnableItem.name)
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
            ScriptableObject.Destroy(itemsToDestroy[i]);
        }
    }

    private static void FixDawnMoonEnemies()
    {
        List<EnemyType> enemiesToDestroy = new();

        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            if (moonInfo.ShouldSkipIgnoreOverride())
                continue;

            foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in moonInfo.Level.Enemies.ToArray())
            {
                if (spawnableEnemyWithRarity.enemyType == null)
                {
                    moonInfo.Level.Enemies.Remove(spawnableEnemyWithRarity);
                    continue;
                }

                bool enemyIsValid = spawnableEnemyWithRarity.enemyType.HasDawnInfo();
                foreach (DawnEnemyInfo enemyInfo in LethalContent.Enemies.Values)
                {
                    if (!enemyIsValid && enemyInfo.EnemyType.name == spawnableEnemyWithRarity.enemyType.name)
                    {
                        Debuggers.Moons?.Log($"replacing fake SO {spawnableEnemyWithRarity.enemyType.name} with {enemyInfo.EnemyType.name}");
                        enemiesToDestroy.Add(spawnableEnemyWithRarity.enemyType);
                        spawnableEnemyWithRarity.enemyType = enemyInfo.EnemyType;
                        break;
                    }
                }
            }

            foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in moonInfo.Level.OutsideEnemies.ToArray())
            {
                if (spawnableEnemyWithRarity.enemyType == null)
                {
                    moonInfo.Level.OutsideEnemies.Remove(spawnableEnemyWithRarity);
                    continue;
                }

                bool enemyIsValid = spawnableEnemyWithRarity.enemyType.HasDawnInfo();
                foreach (DawnEnemyInfo enemyInfo in LethalContent.Enemies.Values)
                {
                    if (!enemyIsValid && enemyInfo.EnemyType.name == spawnableEnemyWithRarity.enemyType.name)
                    {
                        Debuggers.Moons?.Log($"replacing fake SO {spawnableEnemyWithRarity.enemyType.name} with {enemyInfo.EnemyType.name}");
                        enemiesToDestroy.Add(spawnableEnemyWithRarity.enemyType);
                        spawnableEnemyWithRarity.enemyType = enemyInfo.EnemyType;
                        break;
                    }
                }
            }

            foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in moonInfo.Level.DaytimeEnemies.ToArray())
            {
                if (spawnableEnemyWithRarity.enemyType == null)
                {
                    moonInfo.Level.DaytimeEnemies.Remove(spawnableEnemyWithRarity);
                    continue;
                }

                bool enemyIsValid = spawnableEnemyWithRarity.enemyType.HasDawnInfo();
                foreach (DawnEnemyInfo enemyInfo in LethalContent.Enemies.Values)
                {
                    if (!enemyIsValid && enemyInfo.EnemyType.name == spawnableEnemyWithRarity.enemyType.name)
                    {
                        Debuggers.Moons?.Log($"replacing fake SO {spawnableEnemyWithRarity.enemyType.name} with {enemyInfo.EnemyType.name}");
                        enemiesToDestroy.Add(spawnableEnemyWithRarity.enemyType);
                        spawnableEnemyWithRarity.enemyType = enemyInfo.EnemyType;
                        break;
                    }
                }
            }
        }


        for (int i = enemiesToDestroy.Count - 1; i >= 0; i--)
        {
            ScriptableObject.Destroy(enemiesToDestroy[i]);
        }
    }

    // todo: i eventually want to rewrite this so its more extensible and a lot better, but oh well!
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

        List<MoonGroup> groups = _groupAlgorithm.Group(validMoons);

        foreach (MoonGroup group in groups)
        {
            builder.AppendLine("");

            if (!string.IsNullOrEmpty(group.GroupName))
            {
                builder.AppendLine(group.GroupName);
            }

            foreach (DawnMoonInfo moonInfo in group.Moons)
            {
                TerminalPurchaseResult result = moonInfo.PurchasePredicate.CanPurchase();

                if (result is TerminalPurchaseResult.HiddenPurchaseResult)
                {
                    continue;
                }

                builder.AppendLine(FormatMoonEntry(moonInfo, result));
            }
        }

        return orig(self, builder.ToString(), node);
    }

    static string FormatMoonEntry(DawnMoonInfo moonInfo, TerminalPurchaseResult result)
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
            DawnWeatherEffectInfo? currentWeather = moonInfo.GetCurrentWeather();

            if (currentWeather != null)
            {
                builder.Append($"({moonInfo.Level.currentWeather.ToString()})");
            }
        }

        return builder.ToString();
    }

    private static IEnumerator DelayTravelEffects(On.StartOfRound.orig_TravelToLevelEffects orig, StartOfRound self)
    {
        // why
        IEnumerator enumerator = orig(self);
        while (enumerator.MoveNext())
        {
            yield return enumerator.Current;
            if (enumerator.Current is WaitForSeconds wfs && Mathf.Approximately(wfs.m_Seconds, self.currentLevel.timeToArrive))
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
        yield return new WaitUntil(() => DawnMoonNetworker.Instance != null);
        DawnMoonNetworker.Instance!.HostDecide(level.GetDawnInfo());
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
            if (level.HasDawnInfo())
                continue;

            Debuggers.Moons?.Log($"Registering level: {level.PlanetName} with scrap spawn range of: {level.minScrap} and {level.maxScrap}");
            NamespacedKey<DawnMoonInfo>? key = MoonKeys.GetByReflection(NamespacedKey.NormalizeStringForNamespacedKey(level.PlanetName, true).RemoveEnd("Level"));
            if (key == null && LethalLevelLoaderCompat.Enabled && LethalLevelLoaderCompat.TryGetExtendedLevelModName(level, out string moonModName))
            {
                key = NamespacedKey<DawnMoonInfo>.From(NamespacedKey.NormalizeStringForNamespacedKey(moonModName, false), NamespacedKey.NormalizeStringForNamespacedKey(level.PlanetName, false));
            }
            else if (key == null)
            {
                key = NamespacedKey<DawnMoonInfo>.From("unknown_modded", NamespacedKey.NormalizeStringForNamespacedKey(level.PlanetName, false));
            }

            HashSet<NamespacedKey> tags = [DawnLibTags.IsExternal];
            CollectLLLTags(level, tags);

            TerminalNode? routeNode = null;
            TerminalKeyword? nameKeyword = null;
            foreach (CompatibleNoun compatibleNoun in TerminalRefs.RouteKeyword.compatibleNouns)
            {
                if (compatibleNoun.result.displayPlanetInfo == level.levelID)
                {
                    routeNode = compatibleNoun.result;
                    nameKeyword = compatibleNoun.noun;
                    break;
                }
            }

            ITerminalPurchasePredicate predicate = ITerminalPurchasePredicate.AlwaysSuccess();
            if (LethalLevelLoaderCompat.Enabled && LethalLevelLoaderCompat.ExtendedLevelIsModded(level, out object? extendedLevel))
            {
                predicate = new LethalLevelLoaderTerminalPredicate(extendedLevel);
            }
            else if (Equals(key, MoonKeys.Embrion) || Equals(key, MoonKeys.Artifice))
            {
                predicate = new ConstantTerminalPredicate(TerminalPurchaseResult.Hidden().SetFailure(false));
            }

            DawnMoonInfo moonInfo = new DawnMoonInfo(key, tags, level, new([new VanillaMoonSceneInfo(key.AsTyped<IMoonSceneInfo>(), level.sceneName)]), routeNode, null, nameKeyword, new SimpleProvider<int>(routeNode?.itemCost ?? -1), predicate, null);
            level.SetDawnInfo(moonInfo);
            LethalContent.Moons.Register(moonInfo);
        }
        TerminalRefs.MoonCatalogueNode.displayText = "[moonCatalogue]";
    }

    private static void CollectTestLevel(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        if (LethalContent.Moons.IsFrozen)
        {
            orig(self);
            return;
        }

        DawnMoonInfo testMoonInfo = new(MoonKeys.Test, [DawnLibTags.IsExternal], self.currentLevel, new(), null, null, null, new SimpleProvider<int>(-1), ITerminalPurchasePredicate.AlwaysHide(), null);
        self.currentLevel.SetDawnInfo(testMoonInfo);
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
        int cost = moonInfo.Cost.Provide();
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