using System.Collections.Generic;
using System.Linq;
using Dawn.Internal;
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
            new SimpleAutoTagger<CRMoonInfo>(Tags.Company, moonInfo => !moonInfo.Level.spawnEnemiesAndScrap),
            new SimpleAutoTagger<CRMoonInfo>(Tags.Free, moonInfo => moonInfo.RouteNode && moonInfo.RouteNode.itemCost == 0),
            new SimpleAutoTagger<CRMoonInfo>(Tags.Paid, moonInfo => moonInfo.RouteNode && moonInfo.RouteNode.itemCost > 0)
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

        Terminal terminal = GameObject.FindFirstObjectByType<Terminal>();
        TerminalKeyword routeKeyword = terminal.terminalNodes.allKeywords.First(keyword => keyword.word == "route");
        foreach (SelectableLevel level in self.levels)
        {
            Debuggers.Moons?.Log($"Registering level: {level.PlanetName}");
            NamespacedKey<CRMoonInfo>? key = MoonKeys.GetByReflection(NamespacedKey.NormalizeStringForNamespacedKey(level.PlanetName, true).RemoveEnd("Level"));
            if (key == null && LLLCompat.Enabled && LLLCompat.IsExtendedLevel(level))
            {
                key = NamespacedKey<CRMoonInfo>.From("lethal_level_loader", NamespacedKey.NormalizeStringForNamespacedKey(level.PlanetName, false));
            }
            else if (key == null)
            {
                key = NamespacedKey<CRMoonInfo>.From("unknown_modded", NamespacedKey.NormalizeStringForNamespacedKey(level.PlanetName, false));
            }

            List<NamespacedKey> tags = [CRLibTags.IsExternal];
            if (LLLCompat.Enabled && LLLCompat.TryGetAllTagsWithModNames(level, out List<(string modName, string tagName)> tagsWithModNames))
            {
                foreach ((string modName, string tagName) in tagsWithModNames)
                {
                    string normalizedModName = NamespacedKey.NormalizeStringForNamespacedKey(modName, false);
                    string normalizedTagName = NamespacedKey.NormalizeStringForNamespacedKey(tagName, false);

                    if (normalizedModName == "lethalcompany")
                    {
                        normalizedModName = "lethal_level_loader";
                    }
                    Debuggers.Moons?.Log($"Adding tag {normalizedModName}:{normalizedTagName} to level {level.PlanetName}");
                    tags.Add(NamespacedKey.From(normalizedModName, normalizedTagName));
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
            CRMoonInfo moonInfo = new CRMoonInfo(key, tags, level, routeNode, nameKeyword);
            level.SetCRInfo(moonInfo);
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

        CRMoonInfo testMoonInfo = new(MoonKeys.Test, [CRLibTags.IsExternal], self.currentLevel, null, null);
        self.currentLevel.SetCRInfo(testMoonInfo);
        LethalContent.Moons.Register(testMoonInfo);
        orig(self);
    }
}