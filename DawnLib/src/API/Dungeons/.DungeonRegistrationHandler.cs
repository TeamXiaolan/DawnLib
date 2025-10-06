using System;
using System.Collections.Generic;
using System.Linq;
using Dawn.Internal;
using DunGen;
using DunGen.Graph;
using MonoMod.RuntimeDetour;

namespace Dawn;

static class DungeonRegistrationHandler
{
    internal static void Init()
    {
        using (new DetourContext(priority: int.MaxValue))
        {
            On.StartOfRound.Awake += RegisterDawnDungeons;
        }

        LethalContent.Moons.OnFreeze += CollectNonDawnDungeons;
        LethalContent.Moons.OnFreeze += AddDawnDungeonsToMoons;
        On.StartOfRound.SetPlanetsWeather += UpdateAllDungeonWeights;
        On.DunGen.RuntimeDungeon.Start += (orig, self) =>
        {
            self.GenerateOnStart = false;
            orig(self);
        };

        On.DunGen.RuntimeDungeon.Generate += (orig, self) =>
        {
            TryInjectTileSets(self.Generator.DungeonFlow);
            orig(self);
        };
    }

    private static void UpdateAllDungeonWeights(On.StartOfRound.orig_SetPlanetsWeather orig, StartOfRound self, int connectedPlayersOnServer)
    {
        orig(self, connectedPlayersOnServer);
        UpdateDungeonWeightOnLevel(self.currentLevel);
    }

    internal static void UpdateDungeonWeightOnLevel(SelectableLevel level)
    {
        if (!LethalContent.Dungeons.IsFrozen || StartOfRound.Instance == null || (WeatherRegistryCompat.Enabled && !WeatherRegistryCompat.IsWeatherManagerReady()))
            return;

        foreach (IntWithRarity intWithRarity in level.dungeonFlowTypes)
        {
            DawnDungeonInfo dungeonInfo = RoundManagerRefs.Instance.dungeonFlowTypes[intWithRarity.id].dungeonFlow.GetDawnInfo();
            if (dungeonInfo.ShouldSkipRespectOverride())
                continue;

            intWithRarity.rarity = dungeonInfo.Weights?.GetFor(level.GetDawnInfo()) ?? 0;
        }
    }

    private static void AddDawnDungeonsToMoons()
    {
        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            if (moonInfo.ShouldSkipIgnoreOverride())
                continue;

            List<IntWithRarity> intsWithRarity = moonInfo.Level.dungeonFlowTypes.ToList();
            foreach (DawnDungeonInfo dungeonInfo in LethalContent.Dungeons.Values)
            {
                if (dungeonInfo.ShouldSkipIgnoreOverride())
                    continue;

                int id = Array.IndexOf(RoundManagerRefs.Instance.dungeonFlowTypes.Select(t => t.dungeonFlow).ToArray(), dungeonInfo.DungeonFlow);
                IntWithRarity intWithRarity = new()
                {
                    id = id,
                    rarity = 0
                };
                intsWithRarity.Add(intWithRarity);
            }
            moonInfo.Level.dungeonFlowTypes = intsWithRarity.ToArray();
        }
    }

    private static void RegisterDawnDungeons(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        List<IndoorMapType> newIndoorMapTypes = RoundManagerRefs.Instance.dungeonFlowTypes.ToList();
        foreach (DawnDungeonInfo dungeonInfo in LethalContent.Dungeons.Values)
        {
            IndoorMapType indoorMapType = new()
            {
                dungeonFlow = dungeonInfo.DungeonFlow,
                MapTileSize = dungeonInfo.MapTileSize,
                firstTimeAudio = dungeonInfo.FirstTimeAudio,
            };
            newIndoorMapTypes.Add(indoorMapType);
        }
        RoundManagerRefs.Instance.dungeonFlowTypes = newIndoorMapTypes.ToArray();
        orig(self);
    }

    private static void CollectNonDawnDungeons()
    {
        Dictionary<DungeonFlow, WeightTableBuilder<DawnMoonInfo>> dungeonWeightBuilder = new();
        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            if (!moonInfo.ShouldSkipIgnoreOverride())
                continue;

            SelectableLevel level = moonInfo.Level;

            foreach (IntWithRarity intWithRarity in level.dungeonFlowTypes)
            {
                DawnDungeonInfo dungeonInfo = RoundManagerRefs.Instance.dungeonFlowTypes[intWithRarity.id].dungeonFlow.GetDawnInfo();
                if (!dungeonWeightBuilder.TryGetValue(dungeonInfo.DungeonFlow, out WeightTableBuilder<DawnMoonInfo> weightTableBuilder))
                {
                    weightTableBuilder = new WeightTableBuilder<DawnMoonInfo>();
                    dungeonWeightBuilder[dungeonInfo.DungeonFlow] = weightTableBuilder;
                }
                Debuggers.Dungeons?.Log($"Grabbing weight {intWithRarity.rarity} to {dungeonInfo.DungeonFlow.name} on level {level.PlanetName}");
                weightTableBuilder.AddWeight(moonInfo.TypedKey, intWithRarity.rarity);
            }
        }

        foreach (IndoorMapType indoorMapType in RoundManagerRefs.Instance.dungeonFlowTypes)
        {
            if (indoorMapType == null)
                continue;

            if (indoorMapType.dungeonFlow == null)
                continue;

            if (indoorMapType.dungeonFlow.HasDawnInfo())
                continue;

            string name = FormatFlowName(indoorMapType.dungeonFlow);
            NamespacedKey<DawnDungeonInfo>? key = DungeonKeys.GetByReflection(name);
            if (key == null && LethalLevelLoaderCompat.Enabled && LethalLevelLoaderCompat.TryGetExtendedDungeonModName(indoorMapType.dungeonFlow, out string dungeonModName))
            {
                key = NamespacedKey<DawnDungeonInfo>.From(NamespacedKey.NormalizeStringForNamespacedKey(dungeonModName, false), NamespacedKey.NormalizeStringForNamespacedKey(indoorMapType.dungeonFlow.name, false));
            }
            else if (key == null)
            {
                key = NamespacedKey<DawnDungeonInfo>.From("unknown_modded", NamespacedKey.NormalizeStringForNamespacedKey(indoorMapType.dungeonFlow.name, false));
            }

            if (LethalContent.Dungeons.ContainsKey(key))
            {
                Debuggers.Dungeons?.Log($"LethalContent.Dungeons already contains {key}");
                indoorMapType.dungeonFlow.SetDawnInfo(LethalContent.Dungeons[key]);
                continue;
            }

            HashSet<NamespacedKey> tags = [DawnLibTags.IsExternal];
            CollectLLLTags(indoorMapType.dungeonFlow, tags);

            dungeonWeightBuilder.TryGetValue(indoorMapType.dungeonFlow, out WeightTableBuilder<DawnMoonInfo>? weightTableBuilder);
            weightTableBuilder ??= new WeightTableBuilder<DawnMoonInfo>();

            DawnDungeonInfo dungeonInfo = new(key, tags, indoorMapType.dungeonFlow, weightTableBuilder.Build(), indoorMapType.MapTileSize, indoorMapType.firstTimeAudio, null);
            indoorMapType.dungeonFlow.SetDawnInfo(dungeonInfo);
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
                if (tileSet.ShouldSkipIgnoreOverride())
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