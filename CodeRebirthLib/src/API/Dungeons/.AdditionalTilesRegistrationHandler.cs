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
            if (LLLCompat.Enabled && LLLCompat.IsExtendedDungeon(dungeonFlow))
            {
                key = NamespacedKey<CRDungeonInfo>.From("lethal_level_loader", NamespacedKey.NormalizeStringForNamespacedKey(dungeonFlow.name, false));
            }
            else
            {
                key = NamespacedKey<CRDungeonInfo>.From("unknown_modded", NamespacedKey.NormalizeStringForNamespacedKey(dungeonFlow.name, false));
            }

            List<NamespacedKey> tags = [CRLibTags.IsExternal];

            CollectLLLTags(dungeonFlow, tags);
            CRDungeonInfo dungeonInfo = new(key, tags, dungeonFlow);
            dungeonFlow.SetCRInfo(dungeonInfo);
            LethalContent.Dungeons.Register(dungeonInfo);
        }
        
        CollectArchetypesAndTileSets();
        LethalContent.Dungeons.Freeze();
    }
    private static void CollectArchetypesAndTileSets() {
        foreach (CRDungeonInfo dungeonInfo in LethalContent.Dungeons.Values)
        {
            foreach (DungeonArchetype dungeonArchetype in dungeonInfo.DungeonFlow.GetUsedArchetypes())
            {
                NamespacedKey<CRArchetypeInfo> archetypeKey = NamespacedKey<CRArchetypeInfo>.From(dungeonInfo.Key.Namespace, NamespacedKey.NormalizeStringForNamespacedKey(dungeonArchetype.name, false)); // todo: ArchetypeKeys
                if (LethalContent.Archetypes.ContainsKey(archetypeKey))
                {
                    Debuggers.Dungeons?.Log($"LethalContent.TileSets already contains {archetypeKey}");
                    continue;
                }
                
                CRArchetypeInfo info = new CRArchetypeInfo(archetypeKey, [CRLibTags.IsExternal], dungeonArchetype);
                info.ParentInfo = dungeonInfo;
                LethalContent.Archetypes.Register(info);

                IEnumerable<TileSet> allTiles = [..dungeonArchetype.TileSets, ..dungeonArchetype.BranchCapTileSets];
                foreach (TileSet tileSet in allTiles)
                {
                    NamespacedKey<CRTileSetInfo> tileSetKey = NamespacedKey<CRTileSetInfo>.From(dungeonInfo.Key.Namespace, NamespacedKey.NormalizeStringForNamespacedKey(tileSet.name, false)); // todo
                    if (LethalContent.TileSets.ContainsKey(tileSetKey))
                    {
                        Debuggers.Dungeons?.Log($"LethalContent.TileSets already contains {tileSetKey}");
                        continue;
                    }
                    CRTileSetInfo tileSetInfo = new CRTileSetInfo(tileSetKey, [CRLibTags.IsExternal], tileSet, dungeonArchetype.BranchCapTileSets.Contains(tileSet), dungeonArchetype.TileSets.Contains(tileSet));
                    info.AddTileSet(tileSetInfo);
                    LethalContent.TileSets.Register(tileSetInfo);
                }
            }
        }
        
        LethalContent.Archetypes.Freeze();
        LethalContent.TileSets.Freeze();
    }
    private static void CollectLLLTags(DungeonFlow dungeonFlow, List<NamespacedKey> tags) {
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

            CollectLLLTags(dungeonFlow, tags);
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