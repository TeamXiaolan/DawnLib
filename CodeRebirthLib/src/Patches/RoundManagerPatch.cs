using System.Collections.Generic;
using System.Linq;
using CodeRebirthLib.ContentManagement.MapObjects;
using CodeRebirthLib.Extensions;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

namespace CodeRebirthLib.Patches;
static class RoundManagerPatch
{
    internal static List<RegisteredCRMapObject<SpawnableOutsideObject>> registeredOutsideObjects = [];

    internal static void Init()
    {
        On.RoundManager.SpawnOutsideHazards += SpawnOutsideMapObjects;
        On.RoundManager.SpawnMapObjects += RoundManager_SpawnMapObjects;
    }

    private static void RoundManager_SpawnMapObjects(On.RoundManager.orig_SpawnMapObjects orig, RoundManager self)
    {
        RandomMapObject[] randomMapObjects = UnityEngine.Object.FindObjectsOfType<RandomMapObject>();

        foreach (RandomMapObject randomMapObject in randomMapObjects)
        {
            foreach (RegisteredCRMapObject<SpawnableMapObject> mapObject in StartOfRoundPatch.registeredInsideMapObjects)
            {
                if (randomMapObject.spawnablePrefabs.Any((prefab) => prefab == mapObject.MapObject.prefabToSpawn))
                    continue;

                randomMapObject.spawnablePrefabs.Add(mapObject.MapObject.prefabToSpawn);
            }
        }
        orig(self);
    }

    private static void SpawnOutsideMapObjects(On.RoundManager.orig_SpawnOutsideHazards orig, RoundManager self)
    {
        orig(self);

        Random random = new(StartOfRound.Instance.randomMapSeed + 69);
        foreach (RegisteredCRMapObject<SpawnableOutsideObject> registeredOutsideObject in registeredOutsideObjects)
        {
            if (registeredOutsideObject.MapObject == null)
                continue;

            HandleSpawningOutsideObjects(registeredOutsideObject, random); // there isn't an inside version because those are handled on StartOfRound's Start/Awake, this is because vanilla lacks some features in handling outside objects so I have to do it myself.
        }
    }

    private static void HandleSpawningOutsideObjects(RegisteredCRMapObject<SpawnableOutsideObject> outsideObjDef, Random random)
    {
        SelectableLevel level = RoundManager.Instance.currentLevel;
        AnimationCurve animationCurve = new(new Keyframe(0, 0), new Keyframe(1, 0));
        GameObject prefabToSpawn = outsideObjDef.MapObject.prefabToSpawn;
        animationCurve = outsideObjDef.SpawnRateFunction(level);

        int randomNumberToSpawn;
        if (outsideObjDef.HasNetworkObject)
        {
            if (!NetworkManager.Singleton.IsServer)
                return;

            float number = animationCurve.Evaluate(UnityEngine.Random.Range(0f, 1f)) + 0.5f;
            CodeRebirthLibPlugin.ExtendedLogging($"number generated for host only: {number}");
            randomNumberToSpawn = Mathf.FloorToInt(number);
        }
        else
        {
            float number = animationCurve.Evaluate(random.NextFloat(0f, 1f)) + 0.5f;
            CodeRebirthLibPlugin.ExtendedLogging("number generated for everyone: " + number);
            randomNumberToSpawn = Mathf.FloorToInt(number);
        }

        CodeRebirthLibPlugin.ExtendedLogging($"Spawning {randomNumberToSpawn} of {prefabToSpawn.name} for level {level}");
        for (int i = 0; i < randomNumberToSpawn; i++)
        {
            Vector3 spawnPos;
            if (outsideObjDef.HasNetworkObject)
            {
                spawnPos = RoundManager.Instance.outsideAINodes[UnityEngine.Random.Range(0, RoundManager.Instance.outsideAINodes.Length)].transform.position; // TODO a lot of these UnityEngine's need to be changed to use a host seed of some sort so that seeds can fully replicate the whole level
                spawnPos = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(spawnPos, 10f, default, new System.Random(UnityEngine.Random.Range(0, 10000)), -1) + (Vector3.up * 2);
            }
            else
            {
                spawnPos = RoundManager.Instance.outsideAINodes[random.Next(RoundManager.Instance.outsideAINodes.Length)].transform.position;
                spawnPos = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(spawnPos, 10f, default, random, -1) + (Vector3.up * 2);
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