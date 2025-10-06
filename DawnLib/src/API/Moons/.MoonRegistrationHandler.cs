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
            new SimpleAutoTagger<DawnMoonInfo>(Tags.Paid, moonInfo => moonInfo.RouteNode && moonInfo.RouteNode!.itemCost > 0)
        );

        using (new DetourContext(priority: int.MaxValue))
        {
            On.StartOfRound.Awake += CollectLevels;
            On.QuickMenuManager.Start += CollectLevels;
        }

        On.StartOfRound.ChangeLevel += StartOfRoundOnChangeLevel;
        On.StartOfRound.OnClientConnect += StartOfRoundOnClientConnect;
        On.StartOfRound.OnClientDisconnect += StartOfRoundOnClientDisconnect;

        On.StartOfRound.TravelToLevelEffects += DelayTravelEffects;

        On.Terminal.TextPostProcess += DynamicMoonCatalogue;
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

        if (moonInfo.HasTag(Tags.Company))
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

    private static void CollectLevels(On.QuickMenuManager.orig_Start orig, QuickMenuManager self)
    {
        orig(self);
        Terminal terminal = TerminalRefs.Instance;
        TerminalKeyword routeKeyword = TerminalRefs.RouteKeyword;
        List<CompatibleNoun> routeNouns = routeKeyword.compatibleNouns.ToList();
        List<TerminalKeyword> allKeywords = terminal.terminalNodes.allKeywords.ToList();

        List<SelectableLevel> levels = StartOfRound.Instance.levels.ToList();
        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            if (moonInfo.ShouldSkipIgnoreOverride())
                continue;

            moonInfo.Level.levelID = levels.Count;
            moonInfo.ReceiptNode.buyRerouteToMoon = levels.Count;
            levels.Add(moonInfo.Level);

            UpdateMoonPrice(moonInfo);

            if (!LethalContent.Moons.IsFrozen)
            {
                routeNouns.Add(new CompatibleNoun()
                {
                    noun = moonInfo.NameKeyword,
                    result = moonInfo.RouteNode
                });
                allKeywords.Add(moonInfo.NameKeyword);
                moonInfo.NameKeyword.defaultVerb = routeKeyword;

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
        }
        StartOfRound.Instance.levels = levels.ToArray();
        routeKeyword.compatibleNouns = routeNouns.ToArray();
        terminal.terminalNodes.allKeywords = allKeywords.ToArray();

        if (LethalContent.Moons.IsFrozen)
            return;

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
            foreach (CompatibleNoun compatibleNoun in routeKeyword.compatibleNouns)
            {
                if (compatibleNoun.result.displayPlanetInfo == level.levelID)
                {
                    routeNode = compatibleNoun.result;
                    nameKeyword = compatibleNoun.noun;
                    break;
                }
            }

            ITerminalPurchasePredicate predicate = ITerminalPurchasePredicate.AlwaysSuccess();

            if (Equals(key, MoonKeys.Embrion) || Equals(key, MoonKeys.Artifice))
            {
                predicate = ITerminalPurchasePredicate.AlwaysHide();
            }

            DawnMoonInfo moonInfo = new DawnMoonInfo(key, tags, level, new([new VanillaMoonSceneInfo(key.AsTyped<IMoonSceneInfo>(), level.sceneName)]), routeNode, null, nameKeyword, new SimpleProvider<int>(routeNode?.itemCost ?? -1), predicate, null);
            level.SetDawnInfo(moonInfo);
            LethalContent.Moons.Register(moonInfo);
        }

        TerminalRefs.MoonCatalogueNode.displayText = "[moonCatalogue]";
        LethalContent.Moons.Freeze();
    }

    private static void CollectLevels(On.StartOfRound.orig_Awake orig, StartOfRound self)
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