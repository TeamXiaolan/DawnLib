using System.Collections.Generic;
using System.Linq;
using CodeRebirthLib.ContentManagement.MapObjects;
using CodeRebirthLib.Extensions;
using Unity.Netcode;
using UnityEngine;

namespace CodeRebirthLib.Patches;

static class CRMapObjectsPatch
{
    private static List<CRMapObjectDefinition> _registeredInsideMapObjects => CRMod.AllMapObjects().Where(x => x.InsideSpawnMechanics != null).ToList();
    private static bool _alreadyAddedInsideMapObjects = false;

    internal static void Init()
    {
        On.StartOfRound.Awake += StartOfRound_Awake;
        On.RoundManager.SpawnOutsideHazards += RoundManager_SpawnOutsideHazards;
        On.RoundManager.SpawnMapObjects += RoundManager_SpawnMapObjects;
    }

    private static void StartOfRound_Awake(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        orig(self);
        if (_alreadyAddedInsideMapObjects)
            return;

        foreach (SelectableLevel level in StartOfRound.Instance.levels)
        {
            foreach (var insideMapObject in _registeredInsideMapObjects)
            {
                HandleAddingInsideMapObjectToLevel(insideMapObject, level);
            }
        }
    }

    private static void HandleAddingInsideMapObjectToLevel(CRMapObjectDefinition registeredMapObject, SelectableLevel level)
    {
        AnimationCurve curve = registeredMapObject.InsideSpawnMechanics!.CurveFunction(level);
        if (curve == AnimationCurve.Linear(0, 0, 1, 0))
            return;

        SpawnableMapObject spawnableMapObject = new()
        {
            prefabToSpawn = registeredMapObject.GameObject,
            spawnFacingAwayFromWall = registeredMapObject.InsideMapObjectSettings.spawnFacingAwayFromWall,
            spawnFacingWall = registeredMapObject.InsideMapObjectSettings.spawnFacingWall,
            spawnWithBackToWall = registeredMapObject.InsideMapObjectSettings.spawnWithBackToWall,
            spawnWithBackFlushAgainstWall = registeredMapObject.InsideMapObjectSettings.spawnWithBackFlushAgainstWall,
            requireDistanceBetweenSpawns = registeredMapObject.InsideMapObjectSettings.requireDistanceBetweenSpawns,
            disallowSpawningNearEntrances = registeredMapObject.InsideMapObjectSettings.disallowSpawningNearEntrances,
            numberToSpawn = registeredMapObject.InsideSpawnMechanics!.CurveFunction(level) // this works right?
        };

        level.spawnableMapObjects = level.spawnableMapObjects.Append(spawnableMapObject).ToArray();
        _alreadyAddedInsideMapObjects = true;
        CodeRebirthLibPlugin.ExtendedLogging($"added {registeredMapObject.GameObject.name} to level {level.name}.");
    }

    private static void RoundManager_SpawnMapObjects(On.RoundManager.orig_SpawnMapObjects orig, RoundManager self)
    {
        RandomMapObject[] randomMapObjects = UnityEngine.Object.FindObjectsOfType<RandomMapObject>();

        foreach (RandomMapObject randomMapObject in randomMapObjects)
        {
            foreach (CRMapObjectDefinition mapObject in _registeredInsideMapObjects)
            {
                if (randomMapObject.spawnablePrefabs.Any((prefab) => prefab == mapObject.GameObject))
                    continue;

                randomMapObject.spawnablePrefabs.Add(mapObject.GameObject);
            }
        }
        orig(self);
    }

    internal static List<CRMapObjectDefinition> registeredOutsideObjects => CRMod.AllMapObjects().Where(x => x.OutsideSpawnMechanics != null).ToList();

    private static void RoundManager_SpawnOutsideHazards(On.RoundManager.orig_SpawnOutsideHazards orig, RoundManager self)
    {
        orig(self);

        System.Random everyoneRandom = new(StartOfRound.Instance.randomMapSeed + 69);
        System.Random serverOnlyRandom = new(StartOfRound.Instance.randomMapSeed + 6969);
        foreach (CRMapObjectDefinition registeredOutsideObject in registeredOutsideObjects)
        {
            if (registeredOutsideObject.GameObject == null)
                continue;

            HandleSpawningOutsideObjects(registeredOutsideObject, everyoneRandom, serverOnlyRandom);
            // there isn't an inside version because those are handled on StartOfRound's Start/Awake, this is because vanilla lacks some features in handling outside objects so I have to do it myself.
        }
    }

    private static void HandleSpawningOutsideObjects(CRMapObjectDefinition outsideObjDef, System.Random everyoneRandom, System.Random serverOnlyRandom)
    {
        SelectableLevel level = RoundManager.Instance.currentLevel;
        GameObject prefabToSpawn = outsideObjDef.GameObject;
        AnimationCurve animationCurve = outsideObjDef.OutsideSpawnMechanics!.CurveFunction(level);

        int randomNumberToSpawn;
        if (outsideObjDef.HasNetworkObject)
        {
            if (!NetworkManager.Singleton.IsServer)
                return;

            float number = animationCurve.Evaluate(serverOnlyRandom.NextFloat(0f, 1f)) + 0.5f;
            CodeRebirthLibPlugin.ExtendedLogging($"number generated for host only: {number}");
            randomNumberToSpawn = Mathf.FloorToInt(number);
        }
        else
        {
            float number = animationCurve.Evaluate(everyoneRandom.NextFloat(0f, 1f)) + 0.5f;
            CodeRebirthLibPlugin.ExtendedLogging("number generated for everyone: " + number);
            randomNumberToSpawn = Mathf.FloorToInt(number);
        }

        CodeRebirthLibPlugin.ExtendedLogging($"Spawning {randomNumberToSpawn} of {prefabToSpawn.name} for level {level}");
        for (int i = 0; i < randomNumberToSpawn; i++)
        {
            Vector3 spawnPos;
            if (outsideObjDef.HasNetworkObject)
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
            CodeRebirthLibPlugin.ExtendedLogging($"Spawning {spawnedPrefab.name} at {hit.point}");
            if (outsideObjDef.AlignWithTerrain)
            {
                spawnedPrefab.transform.up = hit.normal;
            }

            if (!outsideObjDef.HasNetworkObject)
                continue;

            spawnedPrefab.GetComponent<NetworkObject>().Spawn(true);
        }
    }
}