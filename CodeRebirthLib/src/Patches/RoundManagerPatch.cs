using System.Collections.Generic;
using CodeRebirthLib.ContentManagement.MapObjects;
using CodeRebirthLib.Extensions;
using Unity.Netcode;
using UnityEngine;

namespace CodeRebirthLib.Patches;
static class RoundManagerPatch
{
    internal static List<RegisteredCRMapObject> registeredMapObjects = [];

    internal static void Patch()
    {
        On.RoundManager.SpawnOutsideHazards += SpawnOutsideMapObjects;
    }

    private static void SpawnOutsideMapObjects(On.RoundManager.orig_SpawnOutsideHazards orig, RoundManager self)
    {
        orig(self);

        System.Random random = new(StartOfRound.Instance.randomMapSeed + 69);
        foreach (RegisteredCRMapObject registeredMapObject in registeredMapObjects)
        {
            HandleSpawningOutsideMapObjects(registeredMapObject, random);
        }
    }
    
    private static void HandleSpawningOutsideMapObjects(RegisteredCRMapObject mapObjDef, System.Random random)
    {
        SelectableLevel level = RoundManager.Instance.currentLevel;
        AnimationCurve animationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
        GameObject prefabToSpawn = mapObjDef.outsideObject.spawnableObject.prefabToSpawn;

        if (mapObjDef.hasNetworkObject && !NetworkManager.Singleton.IsServer)
            return;

        animationCurve = mapObjDef.spawnRateFunction(level);
        int randomNumberToSpawn = Mathf.FloorToInt(animationCurve.Evaluate(random.NextFloat(0f, 1f)) + 0.5f);
        CodeRebirthLibPlugin.ExtendedLogging($"Spawning {randomNumberToSpawn} of {prefabToSpawn.name} for level {level}");
        for (int i = 0; i < randomNumberToSpawn; i++)
        {
            Vector3 spawnPos = RoundManager.Instance.outsideAINodes[random.Next(RoundManager.Instance.outsideAINodes.Length)].transform.position;
            spawnPos = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(spawnPos, 10f, default, random, -1) + (Vector3.up * 2);
            Physics.Raycast(spawnPos, Vector3.down, out RaycastHit hit, 100, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore);
            
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