using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Dawn.Internal;
using Dawn.Utils;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace Dawn;

static class MoonRegistrationHandler
{
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
        On.StartOfRound.OnClientConnect += StartOfRoundOnOnClientConnect;
        On.StartOfRound.OnClientDisconnect += StartOfRoundOnOnClientDisconnect;
        On.StartOfRound.ChangePlanet += StartOfRoundOnChangePlanet;
    }
    private static void StartOfRoundOnChangePlanet(On.StartOfRound.orig_ChangePlanet orig, StartOfRound self)
    {
        try
        {
            orig(self);
        }
        catch (Exception e)
        {
            DawnPlugin.Logger.LogError(e);
        }
    }
    private static void StartOfRoundOnOnClientDisconnect(On.StartOfRound.orig_OnClientDisconnect orig, StartOfRound self, ulong clientid)
    {
        orig(self, clientid);
        
        if (self.IsServer && self.inShipPhase)
        {
            DawnMoonNetworker.Instance.HostRebroadcastQueue();
        }
    }
    private static void StartOfRoundOnOnClientConnect(On.StartOfRound.orig_OnClientConnect orig, StartOfRound self, ulong clientid)
    {
        orig(self, clientid);

        if (self.IsServer && self.inShipPhase)
        {
            DawnMoonNetworker.Instance.HostRebroadcastQueue();
        }
    }
    private static void StartOfRoundOnChangeLevel(On.StartOfRound.orig_ChangeLevel orig, StartOfRound self, int levelid)
    {
        orig(self, levelid);
        
        if(self.IsServer)
            self.StartCoroutine(DoHotloadSceneStuff(self.currentLevel));
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
            if(moonInfo.ShouldSkipIgnoreOverride())
                continue;

            moonInfo.Level.levelID = levels.Count;
            moonInfo.ReceiptNode.buyRerouteToMoon = levels.Count;
            levels.Add(moonInfo.Level);

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
            if(level.HasDawnInfo())
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
            
            // todo: handle passing correct predicate for embrion etc.
            DawnMoonInfo moonInfo = new DawnMoonInfo(key, tags, level, routeNode, null, nameKeyword, new SimpleProvider<int>(routeNode?.itemCost ?? -1), ITerminalPurchasePredicate.AlwaysSuccess(),null);
            level.SetDawnInfo(moonInfo);
            LethalContent.Moons.Register(moonInfo);
        }
        
        LethalContent.Moons.Freeze();
    }

    private static void CollectLevels(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        if (LethalContent.Moons.IsFrozen)
        {
            orig(self);
            return;
        }

        DawnMoonInfo testMoonInfo = new(MoonKeys.Test, [DawnLibTags.IsExternal], self.currentLevel, null, null, null,  new SimpleProvider<int>(-1), ITerminalPurchasePredicate.AlwaysHide(), null);
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
}