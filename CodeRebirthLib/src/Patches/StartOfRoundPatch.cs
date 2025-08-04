using System.Collections.Generic;
using System.Linq;
using CodeRebirthLib.ContentManagement.Achievements;
using CodeRebirthLib.ContentManagement.Enemies;
using CodeRebirthLib.ContentManagement.MapObjects;
using CodeRebirthLib.Extensions;
using CodeRebirthLib.Util;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeRebirthLib.Patches;
static class StartOfRoundPatch
{
    internal static List<CRMapObjectDefinition> registeredInsideMapObjects = [];

    internal static void Init()
    {
        On.StartOfRound.Awake += StartOfRound_Awake;
        On.StartOfRound.Start += StartOfRound_Start;
        On.StartOfRound.SetPlanetsWeather += StartOfRound_SetPlanetsWeather;
        On.StartOfRound.AutoSaveShipData += StartOfRound_AutoSaveShipData;
    }

    private static void StartOfRound_Awake(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        orig(self);
        VanillaLevels.Init();
        foreach (SelectableLevel level in StartOfRound.Instance.levels)
        {
            foreach (var insideMapObject in registeredInsideMapObjects)
            {
                HandleAddingInsideMapObjectToLevel(insideMapObject, level);
            }
        }
    }

    private static void HandleAddingInsideMapObjectToLevel(CRMapObjectDefinition registeredMapObject, SelectableLevel level)
    {
        if (level.spawnableMapObjects.Any(x => x.prefabToSpawn == registeredMapObject.GameObject))
        {
            var list = level.spawnableMapObjects.ToList();
            list.RemoveAll(x => x.prefabToSpawn == registeredMapObject.GameObject);
            level.spawnableMapObjects = list.ToArray();
        }

        SpawnableMapObject spawnableMapObject = new()
        {
            prefabToSpawn = registeredMapObject.GameObject,
            spawnFacingAwayFromWall = registeredMapObject.InsideMapObjectSettings.spawnFacingAwayFromWall,
            spawnFacingWall = registeredMapObject.InsideMapObjectSettings.spawnFacingWall,
            spawnWithBackToWall = registeredMapObject.InsideMapObjectSettings.spawnWithBackToWall,
            spawnWithBackFlushAgainstWall = registeredMapObject.InsideMapObjectSettings.spawnWithBackFlushAgainstWall,
            requireDistanceBetweenSpawns = registeredMapObject.InsideMapObjectSettings.requireDistanceBetweenSpawns,
            disallowSpawningNearEntrances = registeredMapObject.InsideMapObjectSettings.disallowSpawningNearEntrances,
            numberToSpawn = registeredMapObject.OutsideSpawnMechanics!.CurveFunction(level) // this works right?
        };

        var mapObjectsList = level.spawnableMapObjects.ToList();
        mapObjectsList.Add(spawnableMapObject);
        level.spawnableMapObjects = mapObjectsList.ToArray(); // would it be a problem to add it to a level even though the numberToSpawn is certain to be 0 depending on the level?
        CodeRebirthLibPlugin.ExtendedLogging($"added {registeredMapObject.GameObject.name} to level {level.name}.");
    }

    private static void StartOfRound_AutoSaveShipData(On.StartOfRound.orig_AutoSaveShipData orig, StartOfRound self)
    {
        orig(self);
        CRAchievementHandler.SaveAll();
        CodeRebirthLibNetworker.Instance?.SaveCodeRebirthLibData();
    }

    private static void StartOfRound_SetPlanetsWeather(On.StartOfRound.orig_SetPlanetsWeather orig, StartOfRound self, int connectedplayersonserver)
    {
        orig(self, connectedplayersonserver);
        CREnemyDefinition.UpdateAllWeights();
    }

    private static void StartOfRound_Start(On.StartOfRound.orig_Start orig, StartOfRound self)
    {
        CREnemyDefinition.CreateMoonAttributeStacks();
        orig(self);
        MoreLayerMasks.Init();

        self.NetworkObject.OnSpawn(() =>
        {
            if (self.IsServer || self.IsHost)
            {
                if (!CodeRebirthLibNetworker.Instance)
                {
                    GameObject networkerInstance = Object.Instantiate(CodeRebirthLibPlugin.Main.NetworkerPrefab);
                    SceneManager.MoveGameObjectToScene(networkerInstance, self.gameObject.scene);
                    networkerInstance.GetComponent<NetworkObject>().Spawn();
                }
            }

            if (AchievementUIGetCanvas.Instance == null) Object.Instantiate(CodeRebirthLibPlugin.Main.AchievementGetUICanvasPrefab);
        });
    }
}