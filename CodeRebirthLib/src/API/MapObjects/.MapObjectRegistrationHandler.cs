using System.Collections.Generic;
using System.Linq;
using CodeRebirthLib.Internal;
using CodeRebirthLib.Utils;
using Unity.Netcode;
using UnityEngine;

namespace CodeRebirthLib;

static class MapObjectRegistrationHandler
{
    internal static void Init()
    {
        On.StartOfRound.SetPlanetsWeather += UpdateMapObjectSpawnWeights;
        On.RoundManager.SpawnOutsideHazards += SpawnOutsideMapObjects;
        On.RoundManager.SpawnMapObjects += UpdateMapObjectSpawnWeights;
        LethalContent.Moons.OnFreeze += RegisterMapObjects;
    }

    private static void FreezeMapObjectContents()
    {
        Dictionary<GameObject, CurveTableBuilder<CRMoonInfo>> insideWeightsByPrefab = new();
        Dictionary<GameObject, CurveTableBuilder<CRMoonInfo>> outsideWeightsByPrefab = new();

        Dictionary<GameObject, InsideMapObjectSettings> insidePlacementByPrefab = new();
        Dictionary<GameObject, OutsideMapObjectSettings> outsidePlacementByPrefab = new();

        foreach (CRMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            SelectableLevel level = moonInfo.Level;
            foreach (SpawnableMapObject mapObject in level.spawnableMapObjects)
            {
                GameObject? prefab = mapObject.prefabToSpawn;
                if (prefab == null)
                    continue;

                if (!insideWeightsByPrefab.TryGetValue(prefab, out CurveTableBuilder<CRMoonInfo> builder))
                {
                    builder = new CurveTableBuilder<CRMoonInfo>();
                    insideWeightsByPrefab[prefab] = builder;

                    if (!insidePlacementByPrefab.ContainsKey(prefab))
                    {
                        insidePlacementByPrefab[prefab] = new InsideMapObjectSettings()
                        {
                            spawnFacingAwayFromWall = mapObject.spawnFacingAwayFromWall,
                            spawnFacingWall = mapObject.spawnFacingWall,
                            spawnWithBackToWall = mapObject.spawnWithBackToWall,
                            spawnWithBackFlushAgainstWall = mapObject.spawnWithBackFlushAgainstWall,
                            requireDistanceBetweenSpawns = mapObject.requireDistanceBetweenSpawns,
                            disallowSpawningNearEntrances = mapObject.disallowSpawningNearEntrances
                        };
                    }
                }

                builder.AddCurve(moonInfo.TypedKey, mapObject.numberToSpawn);
            }

            foreach (SpawnableOutsideObjectWithRarity outsideMapObject in level.spawnableOutsideObjects)
            {
                SpawnableOutsideObject? spawnable = outsideMapObject.spawnableObject;
                if (spawnable?.prefabToSpawn == null)
                    continue;

                GameObject prefab = spawnable.prefabToSpawn;
                if (!outsideWeightsByPrefab.TryGetValue(prefab, out CurveTableBuilder<CRMoonInfo> builder))
                {
                    builder = new CurveTableBuilder<CRMoonInfo>();
                    outsideWeightsByPrefab[prefab] = builder;

                    if (!outsidePlacementByPrefab.ContainsKey(prefab))
                    {
                        outsidePlacementByPrefab[prefab] = new OutsideMapObjectSettings()
                        {
                            AlignWithTerrain = false,
                        };
                    }
                }

                builder.AddCurve(moonInfo.TypedKey, outsideMapObject.randomAmount);
            }
        }

        Dictionary<GameObject, CRInsideMapObjectInfo> vanillaInsideMapObjectsDict = new();
        foreach (var kvp in insideWeightsByPrefab)
        {
            GameObject prefab = kvp.Key;
            ProviderTable<AnimationCurve?, CRMoonInfo> table = kvp.Value.Build();

            insidePlacementByPrefab.TryGetValue(prefab, out InsideMapObjectSettings mapObjectSettings);
            CRInsideMapObjectInfo insideInfo = new(
                table,
                mapObjectSettings.spawnFacingAwayFromWall,
                mapObjectSettings.spawnFacingWall,
                mapObjectSettings.spawnWithBackToWall,
                mapObjectSettings.spawnWithBackFlushAgainstWall,
                mapObjectSettings.requireDistanceBetweenSpawns,
                mapObjectSettings.disallowSpawningNearEntrances
            );

            vanillaInsideMapObjectsDict[prefab] = insideInfo;
        }

        Dictionary<GameObject, CROutsideMapObjectInfo> vanillaOutsideMapObjectsDict = new();
        foreach (var kvp in outsideWeightsByPrefab)
        {
            GameObject prefab = kvp.Key;
            ProviderTable<AnimationCurve?, CRMoonInfo> table = kvp.Value.Build();
            outsidePlacementByPrefab.TryGetValue(prefab, out OutsideMapObjectSettings mapObjectSettings);
            CROutsideMapObjectInfo outsideInfo = new(
                table,
                mapObjectSettings.AlignWithTerrain
            );
            vanillaOutsideMapObjectsDict[prefab] = outsideInfo;
        }

        List<GameObject> vanillaMapObjects = insideWeightsByPrefab.Keys
            .Concat(outsideWeightsByPrefab.Keys)
            .Distinct()
            .ToList();

        foreach (GameObject mapObject in vanillaMapObjects)
        {
            if (LethalContent.MapObjects.Values.Any(x => x.MapObject == mapObject))
                continue; // TODO This is not that great, pls find something better

            NamespacedKey<CRMapObjectInfo>? key = (NamespacedKey<CRMapObjectInfo>?)typeof(MapObjectKeys).GetField(NamespacedKey.NormalizeStringForNamespacedKey(mapObject.name, true))?.GetValue(null);
            key ??= NamespacedKey<CRMapObjectInfo>.From("modded_please_replace_this_later", NamespacedKey.NormalizeStringForNamespacedKey(mapObject.name, false));

            vanillaInsideMapObjectsDict.TryGetValue(mapObject, out CRInsideMapObjectInfo insideMapObjectInfo);
            vanillaOutsideMapObjectsDict.TryGetValue(mapObject, out CROutsideMapObjectInfo outsideMapObjectInfo);

            CRMapObjectInfo mapObjectInfo = new(key, [CRLibTags.IsExternal], mapObject, insideMapObjectInfo, outsideMapObjectInfo);
            LethalContent.MapObjects.Register(mapObjectInfo);
        }
        LethalContent.MapObjects.Freeze();
    }

    private static void UpdateMapObjectSpawnWeights(On.RoundManager.orig_SpawnMapObjects orig, RoundManager self)
    {
        UpdateInsideMapObjectSpawnWeightsOnLevel(self.currentLevel);
        orig(self);
    }

    private static void SpawnOutsideMapObjects(On.RoundManager.orig_SpawnOutsideHazards orig, RoundManager self)
    { // TODO probably needs a transpiler on SpawnOutsideHazards for potential navmesh regen
        System.Random everyoneRandom = new(StartOfRound.Instance.randomMapSeed + 69);
        System.Random serverOnlyRandom = new(StartOfRound.Instance.randomMapSeed + 6969);
        foreach (CRMapObjectInfo mapObjectInfo in LethalContent.MapObjects.Values)
        {
            var outsideInfo = mapObjectInfo.OutsideInfo;
            if (outsideInfo == null || mapObjectInfo.Key.IsVanilla() || mapObjectInfo.HasTag(CRLibTags.IsExternal))
                continue;

            HandleSpawningOutsideObjects(outsideInfo, everyoneRandom, serverOnlyRandom);
            // TODO register vanilla outside objects
            // there isn't an inside version because those are handled on StartOfRound's Start/Awake, this is because vanilla lacks some features in handling outside objects so I have to do it myself.
        }
        orig(self);
    }

    private static void HandleSpawningOutsideObjects(CROutsideMapObjectInfo outsideInfo, System.Random everyoneRandom, System.Random serverOnlyRandom)
    {
        SelectableLevel level = RoundManager.Instance.currentLevel;
        GameObject prefabToSpawn = outsideInfo.ParentInfo.MapObject;
        AnimationCurve animationCurve = outsideInfo.SpawnWeights.GetFor(LethalContent.Moons[level.ToNamespacedKey()]) ?? AnimationCurve.Constant(0, 1, 0);

        int randomNumberToSpawn;
        if (outsideInfo.ParentInfo.HasNetworkObject)
        {
            if (!NetworkManager.Singleton.IsServer)
                return;

            float number = animationCurve.Evaluate(serverOnlyRandom.NextFloat(0f, 1f)) + 0.5f;
            Debuggers.MapObjects?.Log($"number generated for host only: {number}");
            randomNumberToSpawn = Mathf.FloorToInt(number);
        }
        else
        {
            float number = animationCurve.Evaluate(everyoneRandom.NextFloat(0f, 1f)) + 0.5f;
            Debuggers.MapObjects?.Log("number generated for everyone: " + number);
            randomNumberToSpawn = Mathf.FloorToInt(number);
        }

        Debuggers.MapObjects?.Log($"Spawning {randomNumberToSpawn} of {prefabToSpawn.name} for level {level}");
        for (int i = 0; i < randomNumberToSpawn; i++)
        {
            Vector3 spawnPos;
            if (outsideInfo.ParentInfo.HasNetworkObject)
            {
                spawnPos = RoundManager.Instance.outsideAINodes[serverOnlyRandom.Next(0, RoundManager.Instance.outsideAINodes.Length)].transform.position;
                spawnPos = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(spawnPos, 10f, default, serverOnlyRandom, -1) + (Vector3.up * 2);
            }
            else
            {
                spawnPos = RoundManager.Instance.outsideAINodes[everyoneRandom.Next(RoundManager.Instance.outsideAINodes.Length)].transform.position;
                spawnPos = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(spawnPos, 10f, default, everyoneRandom, -1) + (Vector3.up * 2);
            }

            if (!Physics.Raycast(spawnPos, Vector3.down, out RaycastHit hit, 100, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
                continue;

            if (!hit.collider)
                continue;

            GameObject spawnedPrefab = Object.Instantiate(prefabToSpawn, hit.point, Quaternion.identity, RoundManager.Instance.mapPropsContainer.transform);
            Debuggers.MapObjects?.Log($"Spawning {spawnedPrefab.name} at {hit.point}");
            if (outsideInfo.AlignWithTerrain)
            {
                spawnedPrefab.transform.up = hit.normal;
            }

            if (!outsideInfo.ParentInfo.HasNetworkObject)
                continue;

            spawnedPrefab.GetComponent<NetworkObject>().Spawn(true);
        }
    }

    private static void UpdateMapObjectSpawnWeights(On.StartOfRound.orig_SetPlanetsWeather orig, StartOfRound self, int connectedPlayersOnServer)
    {
        orig(self, connectedPlayersOnServer);
        UpdateInsideMapObjectSpawnWeightsOnLevel(self.currentLevel);
    }

    internal static void UpdateInsideMapObjectSpawnWeightsOnLevel(SelectableLevel level)
    {
        if (!LethalContent.MapObjects.IsFrozen)
            return;

        foreach (CRMapObjectInfo mapObjectInfo in LethalContent.MapObjects.Values)
        {
            CRInsideMapObjectInfo? insideInfo = mapObjectInfo.InsideInfo;
            if (insideInfo == null || mapObjectInfo.Key.IsVanilla() || mapObjectInfo.HasTag(CRLibTags.IsExternal))
                continue;

            Debuggers.MapObjects?.Log($"Updating spawn weight for {mapObjectInfo.MapObject.name} on level {level.name}");
            level.spawnableMapObjects.Where(mapObject => mapObjectInfo.MapObject == mapObject.prefabToSpawn).First().numberToSpawn = insideInfo.SpawnWeights.GetFor(LethalContent.Moons[level.ToNamespacedKey()]);
        }
    }

    private static void RegisterMapObjects()
    {
        foreach (CRMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            List<SpawnableMapObject> newSpawnableMapObjects = moonInfo.Level.spawnableMapObjects.ToList();
            foreach (var mapObjectInfo in LethalContent.MapObjects.Values)
            {
                if (mapObjectInfo.InsideInfo == null || mapObjectInfo.Key.IsVanilla() || mapObjectInfo.HasTag(CRLibTags.IsExternal))
                    continue;

                SpawnableMapObject spawnableMapObject = new()
                {
                    prefabToSpawn = mapObjectInfo.MapObject,
                    spawnFacingAwayFromWall = mapObjectInfo.InsideInfo.SpawnFacingAwayFromWall,
                    spawnFacingWall = mapObjectInfo.InsideInfo.SpawnFacingWall,
                    spawnWithBackFlushAgainstWall = mapObjectInfo.InsideInfo.SpawnWithBackFlushAgainstWall,
                    spawnWithBackToWall = mapObjectInfo.InsideInfo.SpawnWithBackToWall,
                    requireDistanceBetweenSpawns = mapObjectInfo.InsideInfo.RequireDistanceBetweenSpawns,
                    disallowSpawningNearEntrances = mapObjectInfo.InsideInfo.DisallowSpawningNearEntrances,
                    numberToSpawn = AnimationCurve.Constant(0, 1, 0)
                };

                newSpawnableMapObjects.Add(spawnableMapObject);
            }

            moonInfo.Level.spawnableMapObjects = newSpawnableMapObjects.ToArray();
        }
        FreezeMapObjectContents();
    }
}