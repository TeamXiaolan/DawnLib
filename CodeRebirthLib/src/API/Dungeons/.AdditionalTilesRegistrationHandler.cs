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
                Debuggers.Dungeons?.Log($"dungeonArchetype.name: {dungeonArchetype.name}");
                NamespacedKey<CRArchetypeInfo> archetypeKey;
                if (dungeonInfo.Key.IsVanilla())
                {
                    string name = FormatArchetypeName(dungeonArchetype);
                    archetypeKey = DungeonArchetypeKeys.GetByReflection(name);
                    if (archetypeKey == null)
                    {
                        CodeRebirthLibPlugin.Logger.LogWarning($"archetype: '{dungeonArchetype.name}' (part of {dungeonInfo.Key}) is vanilla, but CodeRebirthLib couldn't get a corresponding NamespacedKey!");
                        continue;
                    }
                }
                else
                {
                    archetypeKey = NamespacedKey<CRArchetypeInfo>.From(dungeonInfo.Key.Namespace, NamespacedKey.NormalizeStringForNamespacedKey(dungeonArchetype.name, false));
                }
                
                if (LethalContent.Archetypes.ContainsKey(archetypeKey))
                {
                    Debuggers.Dungeons?.Log($"LethalContent.Archetypes already contains {archetypeKey}");
                    continue;
                }
                
                CRArchetypeInfo info = new CRArchetypeInfo(archetypeKey, [CRLibTags.IsExternal], dungeonArchetype);
                info.ParentInfo = dungeonInfo;
                LethalContent.Archetypes.Register(info);

                IEnumerable<TileSet> allTiles = [..dungeonArchetype.TileSets, ..dungeonArchetype.BranchCapTileSets];
                foreach (TileSet tileSet in allTiles)
                {
                    NamespacedKey<CRTileSetInfo> tileSetKey;
                    Debuggers.Dungeons?.Log($"tileSet.name: {tileSet.name}");
                    if (dungeonInfo.Key.IsVanilla())
                    {
                        string name = FormatTileSetName(tileSet);
                        tileSetKey = DungeonTileSetKeys.GetByReflection(name);
                        if(tileSetKey == null)
                        {
                            CodeRebirthLibPlugin.Logger.LogWarning($"tileset: '{tileSet.name}' (part of {archetypeKey}) is vanilla, but CodeRebirthLib couldn't get a corresponding NamespacedKey!");
                            continue;
                        }
                    }
                    else
                    {
                        tileSetKey = NamespacedKey<CRTileSetInfo>.From(dungeonInfo.Key.Namespace, NamespacedKey.NormalizeStringForNamespacedKey(dungeonArchetype.name, false));
                    }
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
    
    // todo: use this in whatever editor tool generates the vanilla keys.
    private static string FormatTileSetName(TileSet tileSet) {
        string name = NamespacedKey.NormalizeStringForNamespacedKey(tileSet.name, true);
        name = ReplaceInternalLevelNames(name).Replace("Tiles", string.Empty);
        return name;
    }
    private static string FormatArchetypeName(DungeonArchetype dungeonArchetype) {
        string name = NamespacedKey.NormalizeStringForNamespacedKey(dungeonArchetype.name, true);
        name = ReplaceInternalLevelNames(name).Replace("Archetype", string.Empty);
        return name;
    }
    private static string FormatFlowName(DungeonFlow dungeonFlow) {
        string name = NamespacedKey.NormalizeStringForNamespacedKey(dungeonFlow.name, true);
        name = ReplaceInternalLevelNames(name);
        return name;
    }
    
    private static readonly Dictionary<string, string> _internalToHumanDungeonNames = new()
    {
        { "LevelOne", "Facility" },
        { "LevelTwo", "Mansion" },
        { "LevelThree", "Mineshaft" }
    };
    private static string ReplaceInternalLevelNames(string input)
    {
        foreach ((string internalName, string humanName) in _internalToHumanDungeonNames)
        {
            input = input.Replace(internalName, humanName);
        }
        return input;
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

            string name = FormatFlowName(dungeonFlow);
            NamespacedKey<CRDungeonInfo>? key = DungeonKeys.GetByReflection(name);
            if (key == null)
            {
                CodeRebirthLibPlugin.Logger.LogWarning($"{dungeonFlow.name} is vanilla, but CodeRebirthLib couldn't get a corresponding NamespacedKey!");
                continue;
            }

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