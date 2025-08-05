using System.Collections.Generic;
using System.Linq;
using CodeRebirthLib.ContentManagement.MapObjects;
using CodeRebirthLib.Extensions;
using Unity.Netcode;
using UnityEngine;

namespace CodeRebirthLib.Patches;
static class RoundManagerPatch
{
    internal static List<CRMapObjectDefinition> registeredOutsideObjects = [];

    internal static void Init()
    {
        On.RoundManager.SpawnOutsideHazards += RoundManager_SpawnOutsideHazards;
        On.RoundManager.SpawnMapObjects += RoundManager_SpawnMapObjects;
    }

    private static void RoundManager_SpawnMapObjects(On.RoundManager.orig_SpawnMapObjects orig, RoundManager self)
    {
        RandomMapObject[] randomMapObjects = UnityEngine.Object.FindObjectsOfType<RandomMapObject>();

        foreach (RandomMapObject randomMapObject in randomMapObjects)
        {
            foreach (CRMapObjectDefinition mapObject in StartOfRoundPatch.registeredInsideMapObjects)
            {
                if (randomMapObject.spawnablePrefabs.Any((prefab) => prefab == mapObject.GameObject))
                    continue;

                randomMapObject.spawnablePrefabs.Add(mapObject.GameObject);
            }
        }
        orig(self);
    }

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