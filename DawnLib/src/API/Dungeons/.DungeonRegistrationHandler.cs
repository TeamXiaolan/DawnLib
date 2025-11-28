using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dawn.Internal;
using DunGen;
using DunGen.Graph;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using UnityEngine;
using static Dawn.Internal.DawnMoonNetworker;
using static DunGen.Graph.DungeonFlow;

namespace Dawn;

static class DungeonRegistrationHandler
{
    internal static void Init()
    {
        using (new DetourContext(priority: int.MaxValue))
        {
            On.StartOfRound.Awake += RegisterDawnDungeons;
        }

        On.StartOfRound.OnClientDisconnect += StartOfRoundOnClientDisconnect;
        LethalContent.Moons.OnFreeze += AddDawnDungeonsToMoons;
        LethalContent.Moons.OnFreeze += CollectNonDawnDungeons;
        On.StartOfRound.SetPlanetsWeather += UpdateAllDungeonWeights;
        On.StartOfRound.EndOfGame += UnloadDungeonBundleForAllPlayers;
        IL.RoundManager.GenerateNewFloor += DelayDungeonGeneration;
        LethalContent.Dungeons.BeforeFreeze += CleanDawnDungeonReferences;
        On.DunGen.RuntimeDungeon.Start += (orig, self) =>
        {
            self.GenerateOnStart = false;
            orig(self);
        };

        On.DunGen.RuntimeDungeon.Generate += (orig, self) =>
        {
            AdjustFireExits(self.Generator.DungeonFlow);
            TryInjectTileSets(self.Generator.DungeonFlow);
            orig(self);
        };
    }

    private static void CleanDawnDungeonReferences()
    {
        foreach (DawnDungeonInfo dungeonInfo in LethalContent.Dungeons.Values)
        {
            if (dungeonInfo.ShouldSkipIgnoreOverride())
                continue;

            List<DungeonArchetype> archetypes = dungeonInfo.DungeonFlow.GetUsedArchetypes().Distinct().ToList();
            List<TileSet> tileSets = dungeonInfo.DungeonFlow.GetUsedTileSets().Distinct().ToList();
            dungeonInfo.DungeonFlow.Nodes.Clear();
            dungeonInfo.DungeonFlow.Lines.Clear();
            for (int i = archetypes.Count - 1; i >= 0; i--)
            {
                ScriptableObject.Destroy(archetypes[i]);
            }
            for (int i = tileSets.Count - 1; i >= 0; i--)
            {
                ScriptableObject.Destroy(tileSets[i]);
            }
        }
    }

    private static void DelayDungeonGeneration(ILContext il)
    {
        ILCursor c = new(il);

        if (c.TryGotoNext(MoveType.Before,
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<RoundManager>("dungeonGenerator"),
                i => i.MatchCallvirt<RuntimeDungeon>("Generate")))
        {
            c.RemoveRange(3);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<IEnumerator>>(LoadDungeonBundle);

            var startCoroutine = typeof(MonoBehaviour).GetMethod("StartCoroutine", new[] { typeof(IEnumerator) })!;
            c.Emit(OpCodes.Callvirt, startCoroutine);
            c.Emit(OpCodes.Pop);
        }
        else
        {
            DawnPlugin.Logger.LogError("Failed to apply DawnLib dungeon generation delay patch!");
        }
    }

    private static IEnumerator UnloadDungeonBundleForAllPlayers(On.StartOfRound.orig_EndOfGame orig, StartOfRound self, int bodiesInsured, int connectedPlayersOnServer, int scrapCollected)
    {
        IEnumerator unloadIEnumerator = DawnDungeonNetworker.Instance!.UnloadExisting();
        while (unloadIEnumerator.MoveNext())
        {
            yield return unloadIEnumerator.Current;
        }
        DawnDungeonNetworker.Instance.PlayerSetBundleStateServerRpc(GameNetworkManager.Instance.localPlayerController, BundleState.Done);

        IEnumerator origIEnumerator = orig(self, bodiesInsured, connectedPlayersOnServer, scrapCollected);
        while (origIEnumerator.MoveNext())
        {
            yield return origIEnumerator.Current;
        }
    }

    private static IEnumerator LoadDungeonBundle()
    {
        DawnDungeonInfo dungeonInfo = RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.GetDawnInfo();
        if (!dungeonInfo.ShouldSkipIgnoreOverride())
        {
            DawnDungeonNetworker.Instance!.QueueDungeonBundleLoading(dungeonInfo.Key);
            IEnumerator waitForLoad = new WaitUntil(() => DawnDungeonNetworker.Instance!.allPlayersDone);
            while (waitForLoad.MoveNext())
            {
                yield return waitForLoad.Current;
            }
        }
        yield return new WaitForSeconds(0.1f);
        RoundManager.Instance.dungeonGenerator.Generate();
    }

    private static void StartOfRoundOnClientDisconnect(On.StartOfRound.orig_OnClientDisconnect orig, StartOfRound self, ulong clientid)
    {
        orig(self, clientid);

        if (self.IsServer && self.inShipPhase)
        {
            DawnDungeonNetworker.Instance?.HostRebroadcastQueue();
        }
    }

    private static void AdjustFireExits(DungeonFlow dungeonFlow) // code mostly taken from LLL
    {
        EntranceTeleport[] moonEntranceTeleports = GameObject.FindObjectsByType<EntranceTeleport>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Where(e => e.entranceId != 0).ToArray();

        for (int i = 0; i < moonEntranceTeleports.Length; i++)
        {
            EntranceTeleport entranceTeleport = moonEntranceTeleports[i];
            entranceTeleport.entranceId = i + 1;
        }

        foreach (GlobalPropSettings propSettings in dungeonFlow.GlobalProps.Where(p => p.ID == DawnDungeonInfo.FireExitGlobalPropID))
        {
            propSettings.Count = new IntRange(moonEntranceTeleports.Length, moonEntranceTeleports.Length);
        }
    }

    private static void UpdateAllDungeonWeights(On.StartOfRound.orig_SetPlanetsWeather orig, StartOfRound self, int connectedPlayersOnServer)
    {
        // TODO: update on lever pull
        orig(self, connectedPlayersOnServer);
        UpdateDungeonWeightOnLevel(self.currentLevel);
    }

    internal static void UpdateDungeonWeightOnLevel(SelectableLevel level)
    {
        if (!LethalContent.Weathers.IsFrozen || !LethalContent.Dungeons.IsFrozen || StartOfRound.Instance == null || (WeatherRegistryCompat.Enabled && !WeatherRegistryCompat.IsWeatherManagerReady()))
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
            if (dungeonInfo.ShouldSkipIgnoreOverride())
                continue;

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
        Dictionary<string, WeightTableBuilder<DawnMoonInfo>> dungeonWeightBuilder = new();
        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            SelectableLevel level = moonInfo.Level;

            foreach (IntWithRarity intWithRarity in level.dungeonFlowTypes)
            {
                DungeonFlow dungeonFlow = RoundManagerRefs.Instance.dungeonFlowTypes[intWithRarity.id].dungeonFlow;
                if (!dungeonWeightBuilder.TryGetValue(dungeonFlow.name, out WeightTableBuilder<DawnMoonInfo> weightTableBuilder))
                {
                    weightTableBuilder = new WeightTableBuilder<DawnMoonInfo>();
                    dungeonWeightBuilder[dungeonFlow.name] = weightTableBuilder;
                }
                Debuggers.Dungeons?.Log($"Grabbing weight {intWithRarity.rarity} to {dungeonFlow.name} on level {level.PlanetName}");
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

            dungeonWeightBuilder.TryGetValue(indoorMapType.dungeonFlow.name, out WeightTableBuilder<DawnMoonInfo>? weightTableBuilder);
            weightTableBuilder ??= new WeightTableBuilder<DawnMoonInfo>();

            DawnDungeonInfo dungeonInfo = new(key, tags, indoorMapType.dungeonFlow, weightTableBuilder.Build(), indoorMapType.MapTileSize, indoorMapType.firstTimeAudio, string.Empty, null);
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

                HashSet<NamespacedKey> archetypeTags = [];
                if (dungeonInfo.HasTag(DawnLibTags.IsExternal))
                {
                    archetypeTags.Add(DawnLibTags.IsExternal);
                }
                DawnArchetypeInfo info = new DawnArchetypeInfo(archetypeKey, archetypeTags, dungeonArchetype, null);
                dungeonArchetype.SetDawnInfo(info);
                info.ParentInfo = dungeonInfo;
                LethalContent.Archetypes.Register(info);

                List<TileSet> allTiles = [.. dungeonArchetype.TileSets, .. dungeonArchetype.BranchCapTileSets];
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
                        tileSetKey = NamespacedKey<DawnTileSetInfo>.From(dungeonInfo.Key.Namespace, NamespacedKey.NormalizeStringForNamespacedKey(tileSet.name, false));
                    }

                    if (LethalContent.TileSets.ContainsKey(tileSetKey))
                    {
                        Debuggers.Dungeons?.Log($"LethalContent.TileSets already contains {tileSetKey}");
                        tileSet.SetDawnInfo(LethalContent.TileSets[tileSetKey]);
                        continue;
                    }

                    HashSet<NamespacedKey> tileSetTags = [];
                    if (dungeonInfo.HasTag(DawnLibTags.IsExternal))
                    {
                        tileSetTags.Add(DawnLibTags.IsExternal);
                    }
                    DawnTileSetInfo tileSetInfo = new DawnTileSetInfo(tileSetKey, tileSetTags, ConstantPredicate.True, tileSet, dungeonArchetype.BranchCapTileSets.Contains(tileSet), dungeonArchetype.TileSets.Contains(tileSet), null);
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

    private static void TryInjectTileSets(DungeonFlow dungeonFlow)
    {
        foreach (DungeonArchetype archetype in dungeonFlow.GetUsedArchetypes())
        {
            if (archetype == null)
            {
                Debuggers.Dungeons?.Log("Archetype is null in dungeonflow: " + dungeonFlow.name);
                continue;
            }
            Debuggers.Dungeons?.Log($"Injecting tile sets for {archetype.name}");
            foreach (DawnTileSetInfo tileSetInfo in archetype.GetDawnInfo().TileSets)
            {
                if (tileSetInfo.ShouldSkipIgnoreOverride())
                    continue;

                // remove unconditionally.
                archetype.BranchCapTileSets.Remove(tileSetInfo.TileSet);
                archetype.TileSets.Remove(tileSetInfo.TileSet);

                // then if this passes, re-add to the archetype.
                if (!tileSetInfo.InjectionPredicate.Evaluate())
                    continue;

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