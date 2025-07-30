using System.Collections.Generic;
using CodeRebirthLib.ContentManagement.MapObjects;
using CodeRebirthLib.Extensions;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

namespace CodeRebirthLib.Patches;
static class RoundManagerPatch
{
    internal static List<RegisteredCRMapObject> registeredMapObjects = [];

    internal static void Init()
    {
        On.RoundManager.SpawnOutsideHazards += SpawnOutsideMapObjects;
    }

    private static void SpawnOutsideMapObjects(On.RoundManager.orig_SpawnOutsideHazards orig, RoundManager self)
    {
        orig(self);

        Random random = new(StartOfRound.Instance.randomMapSeed + 69);
        foreach (RegisteredCRMapObject registeredMapObject in registeredMapObjects)
        {
            HandleSpawningOutsideMapObjects(registeredMapObject, random);
        }
    }

    private static void HandleSpawningOutsideMapObjects(RegisteredCRMapObject mapObjDef, Random random)
    {
        SelectableLevel level = RoundManager.Instance.currentLevel;
        AnimationCurve animationCurve = new(new Keyframe(0, 0), new Keyframe(1, 0));
        GameObject prefabToSpawn = mapObjDef.outsideObject.spawnableObject.prefabToSpawn;
        animationCurve = mapObjDef.spawnRateFunction(level);

        int randomNumberToSpawn;
        if (mapObjDef.hasNetworkObject)
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
            if (mapObjDef.hasNetworkObject)
            {
                spawnPos = RoundManager.Instance.outsideAINodes[UnityEngine.Random.Range(0, RoundManager.Instance.outsideAINodes.Length)].transform.position;
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
            if (mapObjDef.alignWithTerrain)
            {
                spawnedPrefab.transform.up = hit.normal;
            }
            if (!mapObjDef.hasNetworkObject)
                continue;

            spawnedPrefab.GetComponent<NetworkObject>().Spawn(true);
        }
    }
}