using System.Collections.Generic;
using System.Linq;
using Dawn.Internal;
using EasyTextEffects.Editor.MyBoxCopy.Extensions;
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
            On.StartOfRound.Start += CollectLevels;
        }
    }

    private static void CollectLevels(On.StartOfRound.orig_Start orig, StartOfRound self)
    {
        orig(self);
        if (LethalContent.Moons.IsFrozen)
            return;

        Terminal terminal = TerminalRefs.Instance;
        TerminalKeyword routeKeyword = TerminalRefs.RouteKeyword;
        foreach (SelectableLevel level in self.levels)
        {
            Debuggers.Moons?.Log($"Registering level: {level.PlanetName}");
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
            if (LethalLevelLoaderCompat.Enabled && LethalLevelLoaderCompat.TryGetAllTagsWithModNames(level, out List<(string tagModName, string tagName)> tagsWithModNames))
            {
                foreach ((string tagModName, string tagName) in tagsWithModNames)
                {
                    string normalizedTagModName = NamespacedKey.NormalizeStringForNamespacedKey(tagModName, false);
                    string normalizedTagName = NamespacedKey.NormalizeStringForNamespacedKey(tagName, false);

                    if (normalizedTagModName == "lethalcompany")
                    {
                        normalizedTagModName = "lethal_level_loader";
                    }
                    Debuggers.Moons?.Log($"Adding tag {normalizedTagModName}:{normalizedTagName} to level {level.PlanetName}");
                    tags.Add(NamespacedKey.From(normalizedTagModName, normalizedTagName));
                }
            }

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
            DawnMoonInfo moonInfo = new DawnMoonInfo(key, tags, level, routeNode, nameKeyword, null);
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

        DawnMoonInfo testMoonInfo = new(MoonKeys.Test, [DawnLibTags.IsExternal], self.currentLevel, null, null, null);
        self.currentLevel.SetDawnInfo(testMoonInfo);
        LethalContent.Moons.Register(testMoonInfo);
        orig(self);
    }
}