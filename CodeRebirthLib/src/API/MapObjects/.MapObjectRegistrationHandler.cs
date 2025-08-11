using System.Linq;
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
            // there isn't an inside version because those are handled on StartOfRound's Start/Awake, this is because vanilla lacks some features in handling outside objects so I have to do it myself.
        }
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
            Debuggers.ReplaceThis?.Log($"number generated for host only: {number}");
            randomNumberToSpawn = Mathf.FloorToInt(number);
        }
        else
        {
            float number = animationCurve.Evaluate(everyoneRandom.NextFloat(0f, 1f)) + 0.5f;
            Debuggers.ReplaceThis?.Log("number generated for everyone: " + number);
            randomNumberToSpawn = Mathf.FloorToInt(number);
        }

        Debuggers.ReplaceThis?.Log($"Spawning {randomNumberToSpawn} of {prefabToSpawn.name} for level {level}");
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
            Debuggers.ReplaceThis?.Log($"Spawning {spawnedPrefab.name} at {hit.point}");
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
        foreach (var mapObjectInfo in LethalContent.MapObjects.Values) // TODO, do registration for outside map objects as well
        {
            var insideInfo = mapObjectInfo.InsideInfo;
            if (insideInfo == null || mapObjectInfo.Key.IsVanilla())
                continue;

            level.spawnableMapObjects.Where(mapObject => mapObjectInfo.MapObject == mapObject.prefabToSpawn).First().numberToSpawn = insideInfo.SpawnWeights.GetFor(LethalContent.Moons[level.ToNamespacedKey()]);
        }
    }

    private static void RegisterMapObjects(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        foreach (var level in self.levels)
        {
            foreach (var mapObjectInfo in LethalContent.MapObjects.Values) // TODO, do registration for outside map objects as well
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
                    numberToSpawn = AnimationCurve.Constant(0, 1, 0) // todo: dynamiclly update 
                };

                level.spawnableMapObjects = level.spawnableMapObjects.Append(spawnableMapObject).ToArray();
            }
        }

        // then, before freezing registry, add vanilla content
        if (!LethalContent.MapObjects.IsFrozen) // effectively check for a lobby reload
        {
            // todo?
        }

        LethalContent.MapObjects.Freeze();
        orig(self);
    }
}