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
        On.StartOfRound.Awake += RegisterMapObjects;
        On.StartOfRound.Start += FreezeMapObjectContents;
    }

    private static void FreezeMapObjectContents(On.StartOfRound.orig_Start orig, StartOfRound self)
    {
        orig(self);
        if (LethalContent.MapObjects.IsFrozen)
            return;

        Dictionary<SpawnableMapObject, CurveTableBuilder<CRMoonInfo>> vanillaInsideWeights = new();
        Dictionary<SpawnableOutsideObject, CurveTableBuilder<CRMoonInfo>> vanillaOutsideWeights = new();

        foreach (var level in self.levels)
        {
            foreach (var mapObject in level.spawnableMapObjects)
            {
                if (mapObject.prefabToSpawn != null && !vanillaInsideWeights.ContainsKey(mapObject))
                {
                    vanillaInsideWeights.Add(mapObject, new CurveTableBuilder<CRMoonInfo>());
                }
                vanillaInsideWeights[mapObject].AddCurve(level.ToNamespacedKey(), mapObject.numberToSpawn);
            }

            foreach (var mapObject in level.spawnableOutsideObjects)
            {
                if (mapObject.spawnableObject.prefabToSpawn != null && !vanillaOutsideWeights.ContainsKey(mapObject.spawnableObject))
                {
                    vanillaOutsideWeights.Add(mapObject.spawnableObject, new CurveTableBuilder<CRMoonInfo>());
                }
                vanillaOutsideWeights[mapObject.spawnableObject].AddCurve(level.ToNamespacedKey(), mapObject.randomAmount);
            }
        }

        Dictionary<GameObject, CRInsideMapObjectInfo> vanillaInsideMapObjectsDict = new();
        Dictionary<GameObject, CROutsideMapObjectInfo> vanillaOutsideMapObjectsDict = new();

        foreach (var mapObjectWithCurveTableDict in vanillaInsideWeights)
        {
            var spawnableMapObject = mapObjectWithCurveTableDict.Value.Build();
            CRInsideMapObjectInfo insideMapObjectInfo = new(mapObjectWithCurveTableDict.Value.Build(), mapObjectWithCurveTableDict.Key.spawnFacingAwayFromWall, mapObjectWithCurveTableDict.Key.spawnFacingWall, mapObjectWithCurveTableDict.Key.spawnWithBackToWall, mapObjectWithCurveTableDict.Key.spawnWithBackFlushAgainstWall, mapObjectWithCurveTableDict.Key.requireDistanceBetweenSpawns, mapObjectWithCurveTableDict.Key.disallowSpawningNearEntrances);
            vanillaInsideMapObjectsDict.Add(mapObjectWithCurveTableDict.Key.prefabToSpawn, insideMapObjectInfo);
        }

        foreach (var mapObjectWithCurveTableDict in vanillaOutsideWeights)
        {
            var spawnableMapObject = mapObjectWithCurveTableDict.Value.Build();
            CROutsideMapObjectInfo outsideMapObjectInfo = new(mapObjectWithCurveTableDict.Value.Build(), false);
            vanillaOutsideMapObjectsDict.Add(mapObjectWithCurveTableDict.Key.prefabToSpawn, outsideMapObjectInfo);
        }

        List<GameObject> vanillaMapObjects =
        [
            .. vanillaInsideWeights.Keys.Select(x => x.prefabToSpawn),
            .. vanillaOutsideWeights.Keys.Select(x => x.prefabToSpawn)
        ];

        foreach (var mapObject in vanillaMapObjects)
        {
            NamespacedKey<CRMapObjectInfo>? key = (NamespacedKey<CRMapObjectInfo>?)typeof(MapObjectKeys).GetField(NamespacedKey.NormalizeStringForNamespacedKey(mapObject.name))?.GetValue(null);
            if (key == null)
                continue;

            if (LethalContent.MapObjects.ContainsKey(key))
                continue;

            vanillaInsideMapObjectsDict.TryGetValue(mapObject, out CRInsideMapObjectInfo? insideMapObjectInfo);
            vanillaOutsideMapObjectsDict.TryGetValue(mapObject, out CROutsideMapObjectInfo? outsideMapObjectInfo);

            CRMapObjectInfo mapObjectInfo = new(key, true, mapObject, insideMapObjectInfo, outsideMapObjectInfo);
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
            if (outsideInfo == null || mapObjectInfo.Key.IsVanilla())
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
        foreach (var mapObjectInfo in LethalContent.MapObjects.Values)
        {
            var insideInfo = mapObjectInfo.InsideInfo;
            if (insideInfo == null || mapObjectInfo.Key.IsVanilla() || mapObjectInfo.IsExternal)
                continue;

            level.spawnableMapObjects.Where(mapObject => mapObjectInfo.MapObject == mapObject.prefabToSpawn).First().numberToSpawn = insideInfo.SpawnWeights.GetFor(LethalContent.Moons[level.ToNamespacedKey()]);
        }
    }

    private static void RegisterMapObjects(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        foreach (var level in self.levels)
        {
            var newSpawnableMapObjects = level.spawnableMapObjects.ToList();
            foreach (var mapObjectInfo in LethalContent.MapObjects.Values)
            {
                if (mapObjectInfo.InsideInfo == null || mapObjectInfo.Key.IsVanilla())
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

            level.spawnableMapObjects = newSpawnableMapObjects.ToArray();
        }
        orig(self);
    }
}