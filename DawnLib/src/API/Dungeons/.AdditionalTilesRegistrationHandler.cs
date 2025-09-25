using System.Collections.Generic;
using System.Linq;
using Dawn.Internal;
using DunGen;
using DunGen.Graph;

namespace Dawn;

static class AdditionalTilesRegistrationHandler
{
    internal static void Init()
    {
        On.StartOfRound.Awake += CollectVanillaDungeons;
        On.RoundManager.Start += CollectModdedDungeons;
        On.DunGen.RuntimeDungeon.Generate += (orig, self) =>
        {
            TryInjectTileSets(self.Generator.DungeonFlow);
            orig(self);
        };
    }

    private static void CollectVanillaDungeons(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        if (LethalContent.Dungeons.IsFrozen)
        {
            orig(self);
            return;
        }

        foreach (DungeonFlow dungeonFlow in self.transform.parent.GetComponentInChildren<RoundManager>().dungeonFlowTypes.Select(it => it.dungeonFlow))
        {
            if (dungeonFlow == null)
                continue;

            string name = FormatFlowName(dungeonFlow);
            NamespacedKey<DawnDungeonInfo>? key = DungeonKeys.GetByReflection(name);
            if (key == null)
            {
                DawnPlugin.Logger.LogWarning($"{dungeonFlow.name} is vanilla, but DawnLib couldn't get a corresponding NamespacedKey!");
                continue;
            }

            HashSet<NamespacedKey> tags = [DawnLibTags.IsExternal];

            CollectLLLTags(dungeonFlow, tags);
            DawnDungeonInfo dungeonInfo = new(key, tags, dungeonFlow, null);
            dungeonFlow.SetDawnInfo(dungeonInfo);
            LethalContent.Dungeons.Register(dungeonInfo);
        }
        orig(self);
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

            if (dungeonFlow.HasDawnInfo())
                continue;

            Debuggers.Dungeons?.Log($"Registering potentially modded dungeon: {dungeonFlow.name}");
            NamespacedKey<DawnDungeonInfo> key;
            if (LethalLevelLoaderCompat.Enabled && LethalLevelLoaderCompat.TryGetExtendedDungeonModName(dungeonFlow, out string dungeonModName))
            {
                key = NamespacedKey<DawnDungeonInfo>.From(NamespacedKey.NormalizeStringForNamespacedKey(dungeonModName, false), NamespacedKey.NormalizeStringForNamespacedKey(dungeonFlow.name, false));
            }
            else
            {
                key = NamespacedKey<DawnDungeonInfo>.From("unknown_modded", NamespacedKey.NormalizeStringForNamespacedKey(dungeonFlow.name, false));
            }

            if (LethalContent.Dungeons.ContainsKey(key))
            {
                Debuggers.Dungeons?.Log($"LethalContent.Dungeons already contains {key}");
                dungeonFlow.SetDawnInfo(LethalContent.Dungeons[key]);
                continue;
            }

            HashSet<NamespacedKey> tags = [DawnLibTags.IsExternal];
            CollectLLLTags(dungeonFlow, tags);
            DawnDungeonInfo dungeonInfo = new(key, tags, dungeonFlow, null);
            dungeonFlow.SetDawnInfo(dungeonInfo);
            LethalContent.Dungeons.Register(dungeonInfo);
        }

        CollectArchetypesAndTileSets();
        LethalContent.Dungeons.Freeze();
    }

    private static void CollectArchetypesAndTileSets()
    {
        foreach (DawnDungeonInfo dungeonInfo in LethalContent.Dungeons.Values)
        {
            foreach (DungeonArchetype dungeonArchetype in dungeonInfo.DungeonFlow.GetUsedArchetypes())
            {
                Debuggers.Dungeons?.Log($"dungeonArchetype.name: {dungeonArchetype.name}");
                NamespacedKey<DawnArchetypeInfo>? archetypeKey;
                if (dungeonInfo.Key.IsVanilla())
                {
                    string name = FormatArchetypeName(dungeonArchetype);
                    archetypeKey = DungeonArchetypeKeys.GetByReflection(name);
                    if (archetypeKey == null)
                    {
                        DawnPlugin.Logger.LogWarning($"archetype: '{dungeonArchetype.name}' (part of {dungeonInfo.Key}) is vanilla, but DawnLib couldn't get a corresponding NamespacedKey!");
                        continue;
                    }
                }
                else
                {
                    archetypeKey = NamespacedKey<DawnArchetypeInfo>.From(dungeonInfo.Key.Namespace, NamespacedKey.NormalizeStringForNamespacedKey(dungeonArchetype.name, false));
                }

                if (LethalContent.Archetypes.ContainsKey(archetypeKey))
                {
                    Debuggers.Dungeons?.Log($"LethalContent.Archetypes already contains {archetypeKey}");
                    dungeonArchetype.SetDawnInfo(LethalContent.Archetypes[archetypeKey]);
                    continue;
                }

                DawnArchetypeInfo info = new DawnArchetypeInfo(archetypeKey, [DawnLibTags.IsExternal], dungeonArchetype, null);
                dungeonArchetype.SetDawnInfo(info);
                info.ParentInfo = dungeonInfo;
                LethalContent.Archetypes.Register(info);

                IEnumerable<TileSet> allTiles = [.. dungeonArchetype.TileSets, .. dungeonArchetype.BranchCapTileSets];
                foreach (TileSet tileSet in allTiles)
                {
                    NamespacedKey<DawnTileSetInfo>? tileSetKey;
                    Debuggers.Dungeons?.Log($"tileSet.name: {tileSet.name}");
                    if (dungeonInfo.Key.IsVanilla())
                    {
                        string name = FormatTileSetName(tileSet);
                        tileSetKey = DungeonTileSetKeys.GetByReflection(name);
                        if (tileSetKey == null)
                        {
                            DawnPlugin.Logger.LogWarning($"tileset: '{tileSet.name}' (part of {archetypeKey}) is vanilla, but DawnLib couldn't get a corresponding NamespacedKey!");
                            continue;
                        }
                    }
                    else
                    {
                        tileSetKey = NamespacedKey<DawnTileSetInfo>.From(dungeonInfo.Key.Namespace, NamespacedKey.NormalizeStringForNamespacedKey(dungeonArchetype.name, false));
                    }

                    if (LethalContent.TileSets.ContainsKey(tileSetKey))
                    {
                        Debuggers.Dungeons?.Log($"LethalContent.TileSets already contains {tileSetKey}");
                        tileSet.SetDawnInfo(LethalContent.TileSets[tileSetKey]);
                        continue;
                    }
                    DawnTileSetInfo tileSetInfo = new DawnTileSetInfo(tileSetKey, [DawnLibTags.IsExternal], ConstantPredicate.True, tileSet, dungeonArchetype.BranchCapTileSets.Contains(tileSet), dungeonArchetype.TileSets.Contains(tileSet), null);
                    info.AddTileSet(tileSetInfo);
                    tileSet.SetDawnInfo(tileSetInfo);
                    LethalContent.TileSets.Register(tileSetInfo);
                }
            }
        }

        LethalContent.Archetypes.Freeze();
        LethalContent.TileSets.Freeze();
    }

    private static string FormatTileSetName(TileSet tileSet)
    {
        string name = NamespacedKey.NormalizeStringForNamespacedKey(tileSet.name, true);
        name = ReplaceInternalLevelNames(name).Replace("Tiles", string.Empty);
        return name;
    }

    private static string FormatArchetypeName(DungeonArchetype dungeonArchetype)
    {
        string name = NamespacedKey.NormalizeStringForNamespacedKey(dungeonArchetype.name, true);
        name = ReplaceInternalLevelNames(name).Replace("Archetype", string.Empty);
        return name;
    }

    private static string FormatFlowName(DungeonFlow dungeonFlow)
    {
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

    private static void CollectLLLTags(DungeonFlow dungeonFlow, HashSet<NamespacedKey> tags)
    {
        if (LethalLevelLoaderCompat.Enabled && LethalLevelLoaderCompat.TryGetAllTagsWithModNames(dungeonFlow, out List<(string modName, string tagName)> tagsWithModNames))
        {
            tags.AddToList(tagsWithModNames, Debuggers.Dungeons, dungeonFlow.name);
        }
    }

    internal static void TryInjectTileSets(DungeonFlow dungeonFlow)
    {
        foreach (DungeonArchetype archetype in dungeonFlow.GetUsedArchetypes())
        {
            foreach (DawnTileSetInfo tileSet in archetype.GetDawnInfo().TileSets)
            {
                if (tileSet.HasTag(DawnLibTags.IsExternal))
                    continue;

                // remove unconditionally.
                if (archetype.BranchCapTileSets.Contains(tileSet.TileSet))
                    archetype.BranchCapTileSets.Remove(tileSet.TileSet);

                if (archetype.TileSets.Contains(tileSet.TileSet))
                    archetype.TileSets.Remove(tileSet.TileSet);

                // then if this passes, re-add to the archetype.
                if (!tileSet.InjectionPredicate.Evaluate())
                    continue;

                if (tileSet.IsBranchCap)
                {
                    archetype.BranchCapTileSets.Add(tileSet.TileSet);
                }
                if (tileSet.IsRegular)
                {
                    archetype.TileSets.Add(tileSet.TileSet);
                }
            }
        }
    }
}