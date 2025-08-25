using System;
using System.Collections.Generic;
using System.Linq;
using CodeRebirthLib.Internal;
using CodeRebirthLib.Internal.ModCompats;
using DunGen;
using DunGen.Graph;

namespace CodeRebirthLib;

static class AdditionalTilesRegistrationHandler
{
    internal static void Init()
    {
        On.RoundManager.Awake += CollectVanillaDungeons;
        On.RoundManager.Start += CollectModdedDungeons;
        LethalContent.Dungeons.OnFreeze += RegisterTileSets;
    }

    private static void RegisterTileSets()
    {
        foreach (CRDungeonInfo dungeonInfo in LethalContent.Dungeons.Values)
        {
            DungeonFlow dungeonFlow = dungeonInfo.DungeonFlow;
            TryInjectTileSets(dungeonFlow);
        }
    }

    private static void CollectModdedDungeons(On.RoundManager.orig_Start orig, RoundManager self)
    {
        if (LethalContent.Dungeons.IsFrozen)
        {
            orig(self);
            return;
        }

        orig(self);
        foreach (DungeonFlow dungeonFlow in self.dungeonFlowTypes.Select(it => it.dungeonFlow))
        {
            if (dungeonFlow == null)
                continue;

            if (dungeonFlow.TryGetCRInfo(out _))
                continue;

            Debuggers.Dungeons?.Log($"Registering potentially modded dungeon: {dungeonFlow.name}");
            NamespacedKey<CRDungeonInfo> key;
            if (LLLCompat.Enabled && LLLCompat.TryGetExtendedDungeon(dungeonFlow, out _))
            {
                key = NamespacedKey<CRDungeonInfo>.From("lethal_level_loader", NamespacedKey.NormalizeStringForNamespacedKey(dungeonFlow.name, false));
            }
            else
            {
                key = NamespacedKey<CRDungeonInfo>.From("unknown_modded", NamespacedKey.NormalizeStringForNamespacedKey(dungeonFlow.name, false));
            }

            List<NamespacedKey> tags = [CRLibTags.IsExternal];

            if (LLLCompat.Enabled && LLLCompat.TryGetAllTagsWithModNames(dungeonFlow, out List<(string modName, string tagName)> tagsWithModNames))
            {
                foreach ((string modName, string tagName) in tagsWithModNames)
                {
                    bool alreadyAdded = false;
                    foreach (NamespacedKey tag in tags)
                    {
                        if (tag.Key == tagName)
                        {
                            alreadyAdded = true;
                            break;
                        }
                    }

                    if (alreadyAdded)
                        continue;

                    string normalizedModName = NamespacedKey.NormalizeStringForNamespacedKey(modName, false);
                    string normalizedTagName = NamespacedKey.NormalizeStringForNamespacedKey(tagName, false);
                    Debuggers.Dungeons?.Log($"Adding tag {normalizedModName}:{normalizedTagName} to dungeon {dungeonFlow.name}");
                    tags.Add(NamespacedKey.From(normalizedModName, normalizedTagName));
                }
            }
            CRDungeonInfo dungeonInfo = new(key, tags, dungeonFlow);
            dungeonFlow.SetCRInfo(dungeonInfo);
            LethalContent.Dungeons.Register(dungeonInfo);
        }
        LethalContent.Dungeons.Freeze();
    }

    private static void CollectVanillaDungeons(On.RoundManager.orig_Awake orig, RoundManager self)
    {
        if (LethalContent.Dungeons.IsFrozen)
        {
            orig(self);
            return;
        }

        foreach (DungeonFlow dungeonFlow in self.dungeonFlowTypes.Select(it => it.dungeonFlow))
        {
            if (dungeonFlow == null)
                continue;

            if (dungeonFlow.TryGetCRInfo(out _))
                continue;

            NamespacedKey<CRDungeonInfo>? key = (NamespacedKey<CRDungeonInfo>?)typeof(DungeonKeys).GetField(NamespacedKey.NormalizeStringForNamespacedKey(dungeonFlow.name, true))?.GetValue(null);

            List<NamespacedKey> tags = [CRLibTags.IsExternal];

            if (LLLCompat.Enabled && LLLCompat.TryGetAllTagsWithModNames(dungeonFlow, out List<(string modName, string tagName)> tagsWithModNames))
            {
                foreach ((string modName, string tagName) in tagsWithModNames)
                {
                    bool alreadyAdded = false;
                    foreach (NamespacedKey tag in tags)
                    {
                        if (tag.Key == tagName)
                        {
                            alreadyAdded = true;
                            break;
                        }
                    }

                    if (alreadyAdded)
                        continue;
    
                    string normalizedModName = NamespacedKey.NormalizeStringForNamespacedKey(modName, false);
                    string normalizedTagName = NamespacedKey.NormalizeStringForNamespacedKey(tagName, false);
                    Debuggers.Dungeons?.Log($"Adding tag {normalizedModName}:{normalizedTagName} to dungeon {dungeonFlow.name}");
                    tags.Add(NamespacedKey.From(normalizedModName, normalizedTagName));
                }
            }

            CRDungeonInfo dungeonInfo = new(key, tags, dungeonFlow);
            dungeonFlow.SetCRInfo(dungeonInfo);
            LethalContent.Dungeons.Register(dungeonInfo);
        }
        orig(self);
    }

    internal static void TryInjectTileSets(DungeonFlow dungeonFlow)
    {
        foreach (CRTileSetInfo tileSetInfo in LethalContent.TileSets.Values)
        {
            /*if (!tileSetInfo.AppliedTo.Contains(dungeonFlow.ToNamespacedKey()))
            {
                continue;
            }*/

            foreach (DungeonArchetype archetype in dungeonFlow.GetUsedArchetypes())
            {
                Debuggers.Dungeons?.Log($"Injecting {tileSetInfo.TileSet.name} tileset into {archetype.name}");

                if (tileSetInfo.IsBranchCap)
                {
                    archetype.BranchCapTileSets.Add(tileSetInfo.TileSet);
                }

                if (tileSetInfo.IsRegular)
                {
                    archetype.TileSets.Add(tileSetInfo.TileSet);
                }
            }
        }
    }
}