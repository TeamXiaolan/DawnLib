using System;
using System.Collections.Generic;
using System.Linq;
using Dawn.Internal;
using Dawn.Utils;
using HarmonyLib;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Unity.Netcode;
using UnityEngine;

namespace Dawn;

static class MapObjectRegistrationHandler
{
    private static int _spawnedObjects;

    internal static void Init()
    {
        DawnPlugin.Hooks.Add(new Hook(AccessTools.DeclaredMethod(typeof(RandomMapObject), "Awake"), AddPrefabsToRandomMapObjects));

        On.RoundManager.SpawnOutsideHazards += SpawnOutsideMapObjects;
        IL.RoundManager.SpawnOutsideHazards += RegenerateNavMeshTranspiler;

        On.StartOfRound.SetPlanetsWeather += UpdateMapObjectSpawnWeights;
        On.RoundManager.SpawnMapObjects += UpdateMapObjectSpawnWeights;

        LethalContent.Moons.OnFreeze += RegisterMapObjects;
        LethalContent.MapObjects.OnFreeze += FixMapObjectBlanksOnDawnMoons;
    }

    private static void AddPrefabsToRandomMapObjects(RuntimeILReferenceBag.FastDelegateInvokers.Action<RandomMapObject> orig, RandomMapObject self)
    {
        foreach (DawnMapObjectInfo mapObjectInfo in LethalContent.MapObjects.Values)
        {
            if (mapObjectInfo.InsideInfo == null || mapObjectInfo.ShouldSkipRespectOverride() || !mapObjectInfo.HasNetworkObject)
                continue;

            self.spawnablePrefabs.Add(mapObjectInfo.InsideInfo.IndoorMapHazardType.prefabToSpawn);
        }
        orig(self);
    }

    private static void FixMapObjectBlanksOnDawnMoons()
    {
        List<ScriptableObject> SOsToDelete = new();
        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            if (moonInfo.ShouldSkipIgnoreOverride())
                continue;

            foreach (IndoorMapHazard indoorMapHazard in moonInfo.Level.indoorMapHazards)
            {
                if (indoorMapHazard.hazardType == null)
                    continue;

                foreach (DawnMapObjectInfo mapObjectInfo in LethalContent.MapObjects.Values)
                {
                    if (mapObjectInfo.InsideInfo == null)
                        continue;

                    if (mapObjectInfo.InsideInfo.IndoorMapHazardType.name == indoorMapHazard.hazardType.name)
                    {
                        SOsToDelete.Add(indoorMapHazard.hazardType);
                        indoorMapHazard.hazardType = mapObjectInfo.InsideInfo.IndoorMapHazardType;
                        break;
                    }
                }
            }

            foreach (SpawnableOutsideObjectWithRarity spawnableOutsideObjectWithRarity in moonInfo.Level.spawnableOutsideObjects)
            {
                if (spawnableOutsideObjectWithRarity.spawnableObject == null)
                    continue;

                foreach (DawnMapObjectInfo mapObjectInfo in LethalContent.MapObjects.Values)
                {
                    if (mapObjectInfo.OutsideInfo == null)
                        continue;

                    if (mapObjectInfo.OutsideInfo.SpawnableOutsideObject.name == spawnableOutsideObjectWithRarity.spawnableObject.name)
                    {
                        SOsToDelete.Add(spawnableOutsideObjectWithRarity.spawnableObject);
                        spawnableOutsideObjectWithRarity.spawnableObject = mapObjectInfo.OutsideInfo.SpawnableOutsideObject;
                        break;
                    }
                }
            }
        }

        for (int i = SOsToDelete.Count - 1; i >= 0; i--)
        {
            ScriptableObject.Destroy(SOsToDelete[i]);
        }
    }

    private static void RegenerateNavMeshTranspiler(ILContext il)
    {
        ILCursor c = new ILCursor(il);

        if (!c.TryGotoNext(
            i => i.MatchLdloc(out _), // num2
            i => i.MatchLdcI4(0), // 0
            i => i.MatchBgt(out _) // >
        ))
        {
            DawnPlugin.Logger.LogWarning("Failed to apply RoundManager.SpawnOutsideHazards patch (1)!");
            return;
        }

        if (!c.TryGotoNext(
            // further matching so that it doesn't need to be updated with the game as much hopefully
            i => i.MatchLdstr("OutsideLevelNavMesh")))
        {
            DawnPlugin.Logger.LogWarning("Failed to apply RoundManager.SpawnOutsideHazards patch (2)!");
            return;
        }

        c.GotoPrev(MoveType.Before,
            i => i.MatchLdloc(out _), // num2
            i => i.MatchLdcI4(0), // 0
            i => i.MatchBgt(out _)); // >

        c.Index++;
        c.EmitDelegate((int spawned) => spawned + _spawnedObjects);
    }

    private static void FreezeMapObjectContents()
    {
        Dictionary<IndoorMapHazardType, CurveTableBuilder<DawnMoonInfo, SpawnWeightContext>> insideWeightsByHazardType = new();
        Dictionary<SpawnableOutsideObject, CurveTableBuilder<DawnMoonInfo, SpawnWeightContext>> outsideWeightsByOutsideObject = new();

        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            SelectableLevel selectableLevel = moonInfo.Level;
            foreach (IndoorMapHazard indoorMapHazard in selectableLevel.indoorMapHazards)
            {
                IndoorMapHazardType? indoorMapHazardType = indoorMapHazard.hazardType;
                if (indoorMapHazardType == null || indoorMapHazardType.prefabToSpawn == null)
                {
                    continue;
                }

                if (indoorMapHazardType.HasDawnInfo())
                {
                    continue;
                }

                if (!insideWeightsByHazardType.TryGetValue(indoorMapHazardType, out CurveTableBuilder<DawnMoonInfo, SpawnWeightContext> builder))
                {
                    builder = new CurveTableBuilder<DawnMoonInfo, SpawnWeightContext>();
                    insideWeightsByHazardType[indoorMapHazardType] = builder;
                }

                builder.AddCurve(moonInfo.TypedKey, indoorMapHazard.numberToSpawn);
            }

            foreach (SpawnableOutsideObjectWithRarity outsideMapObject in selectableLevel.spawnableOutsideObjects)
            {
                SpawnableOutsideObject? spawnableOutsideObject = outsideMapObject.spawnableObject;
                if (spawnableOutsideObject == null || spawnableOutsideObject.prefabToSpawn == null)
                {
                    continue;
                }

                if (spawnableOutsideObject.HasDawnInfo())
                {
                    continue;
                }

                if (!outsideWeightsByOutsideObject.TryGetValue(spawnableOutsideObject, out CurveTableBuilder<DawnMoonInfo, SpawnWeightContext> builder))
                {
                    builder = new CurveTableBuilder<DawnMoonInfo, SpawnWeightContext>();
                    outsideWeightsByOutsideObject[spawnableOutsideObject] = builder;
                }

                builder.AddCurve(moonInfo.TypedKey, outsideMapObject.randomAmount);
            }
        }

        Dictionary<GameObject, DawnInsideMapObjectInfo> realInsideMapObjectsDict = new();
        foreach (KeyValuePair<IndoorMapHazardType, CurveTableBuilder<DawnMoonInfo, SpawnWeightContext>> kvp in insideWeightsByHazardType)
        {
            IndoorMapHazardType indoorMapHazardType = kvp.Key;
            ProviderTable<AnimationCurve?, DawnMoonInfo, SpawnWeightContext> table = kvp.Value.Build();
            DawnInsideMapObjectInfo insideInfo = new(
                kvp.Key,
                table
            );

            realInsideMapObjectsDict[indoorMapHazardType.prefabToSpawn] = insideInfo;
        }

        Dictionary<GameObject, DawnOutsideMapObjectInfo> realOutsideMapObjectsDict = new();
        foreach (KeyValuePair<SpawnableOutsideObject, CurveTableBuilder<DawnMoonInfo, SpawnWeightContext>> kvp in outsideWeightsByOutsideObject)
        {
            SpawnableOutsideObject spawnableOutsideObject = kvp.Key;
            ProviderTable<AnimationCurve?, DawnMoonInfo, SpawnWeightContext> table = kvp.Value.Build();
            DawnOutsideMapObjectInfo outsideInfo = new(
                spawnableOutsideObject,
                table,
                false,
                0
            );
            realOutsideMapObjectsDict[spawnableOutsideObject.prefabToSpawn] = outsideInfo;
        }

        List<GameObject> realMapObjects = insideWeightsByHazardType.Keys.Select(x => x.prefabToSpawn)
            .Concat(outsideWeightsByOutsideObject.Keys.Select(x => x.prefabToSpawn))
            .Distinct()
            .ToList();

        foreach (GameObject mapObject in realMapObjects)
        {
            string name = NamespacedKey.NormalizeStringForNamespacedKey(mapObject.name, true);
            NamespacedKey<DawnMapObjectInfo>? key = MapObjectKeys.GetByReflection(name);
            if (key == null && LethalLibCompat.Enabled && LethalLibCompat.TryGetMapObjectFromLethalLib(mapObject, out string lethalLibModName))
            {
                key = NamespacedKey<DawnMapObjectInfo>.From(lethalLibModName, mapObject.name);
            }
            else if (key == null && LethalLevelLoaderCompat.Enabled && LethalLevelLoaderCompat.TryGetExtendedMapObjectModName(mapObject, out string extendedModName))
            {
                key = NamespacedKey<DawnMapObjectInfo>.From(extendedModName, mapObject.name);
            }
            else if (key == null)
            {
                key = NamespacedKey<DawnMapObjectInfo>.From("unknown_lib", mapObject.name);
            }

            realInsideMapObjectsDict.TryGetValue(mapObject, out DawnInsideMapObjectInfo? insideMapObjectInfo);
            realOutsideMapObjectsDict.TryGetValue(mapObject, out DawnOutsideMapObjectInfo? outsideMapObjectInfo);

            DawnMapObjectInfo mapObjectInfo = new(key, [DawnLibTags.IsExternal], insideMapObjectInfo, outsideMapObjectInfo, null);
            if (!mapObject.TryGetComponent(out IIndoorMapHazard _))
            {
                DawnMapObjectNamespacedKeyContainer container = mapObject.AddComponent<DawnMapObjectNamespacedKeyContainer>();
                container.Value = key;
            }

            if (insideMapObjectInfo != null)
            {
                insideMapObjectInfo.IndoorMapHazardType.SetDawnInfo(mapObjectInfo);
            }

            if (outsideMapObjectInfo != null)
            {
                outsideMapObjectInfo.SpawnableOutsideObject.SetDawnInfo(mapObjectInfo);
            }

            if (LethalContent.MapObjects.TryGetValue(mapObjectInfo.TypedKey, out DawnMapObjectInfo existingMapObjectInfo))
            {
                if (insideMapObjectInfo != null)
                {
                    insideMapObjectInfo.IndoorMapHazardType.SetDawnInfo(existingMapObjectInfo);
                }

                if (outsideMapObjectInfo != null)
                {
                    outsideMapObjectInfo.SpawnableOutsideObject.SetDawnInfo(existingMapObjectInfo);
                }
                continue;
            }

            LethalContent.MapObjects.Register(mapObjectInfo);
        }
        LethalContent.MapObjects.Freeze();
    }

    private static void UpdateMapObjectSpawnWeights(On.RoundManager.orig_SpawnMapObjects orig, RoundManager self)
    {
        UpdateIndoorMapHazardSpawnWeightsOnLevel(self.currentLevel);
        orig(self);
    }

    private static void SpawnOutsideMapObjects(On.RoundManager.orig_SpawnOutsideHazards orig, RoundManager self)
    {
        UpdateOutsideMapObjectSpawnWeightsOnLevel(self.currentLevel);

        System.Random everyoneRandom = new(StartOfRound.Instance.randomMapSeed + 69);
        System.Random serverOnlyRandom = new(StartOfRound.Instance.randomMapSeed + 6969);
        List<(DawnOutsideMapObjectInfo outsideMapObjectInfo, Vector3 position)> occupiedPositions = new();
        EntranceTeleport[] entranceTeleports = GameObject.FindObjectsByType<EntranceTeleport>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);
        Transform[] shipSpawnPathPoints = RoundManager.Instance.shipSpawnPathPoints;
        GameObject[] spawnDenialPoints = GameObject.FindGameObjectsWithTag("SpawnDenialPoint");
        GameObject itemShipLandingNode = GameObject.FindGameObjectWithTag("ItemShipLandingNode");

        foreach (DawnMapObjectInfo mapObjectInfo in LethalContent.MapObjects.Values)
        {
            DawnOutsideMapObjectInfo? outsideInfo = mapObjectInfo.OutsideInfo;
            if (outsideInfo == null || mapObjectInfo.ShouldSkipRespectOverride())
                continue;

            if (outsideInfo.ParentInfo == null)
            {
                DawnPlugin.Logger.LogError($"Failed to get outside parent info for {outsideInfo.SpawnableOutsideObject.prefabToSpawn.name}");
                continue;
            }

            HandleSpawningOutsideObjects(outsideInfo, occupiedPositions, everyoneRandom, serverOnlyRandom, entranceTeleports, shipSpawnPathPoints, spawnDenialPoints, itemShipLandingNode);
        }
        orig(self);
        _spawnedObjects = 0;
    }

    private static void HandleSpawningOutsideObjects(DawnOutsideMapObjectInfo outsideInfo, List<(DawnOutsideMapObjectInfo outsideMapObjectInfo, Vector3 position)> occupiedPositions, System.Random everyoneRandom, System.Random serverOnlyRandom, EntranceTeleport[] entranceTeleports, Transform[] shipSpawnPathPoints, GameObject[] spawnDenialPoints, GameObject itemShipLandingNode)
    {
        if (RoundManager.Instance.outsideAINodes.Length <= outsideInfo.MinimumAINodeSpawnRequirement)
        {
            return;
        }

        SelectableLevel level = RoundManager.Instance.currentLevel;
        DawnMapObjectInfo mapObjectInfo = outsideInfo.ParentInfo;

        GameObject prefabToSpawn = outsideInfo.SpawnableOutsideObject.prefabToSpawn;

        SpawnWeightContext ctx = new SpawnWeightContext(
            level.GetDawnInfo(),
            RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.GetDawnInfo(),
            TimeOfDayRefs.GetCurrentWeatherEffect(level)?.GetDawnInfo())
            .WithExtra(SpawnWeightExtraKeys.RoutingPriceKey, level.GetDawnInfo().DawnPurchaseInfo.Cost.Provide());

        AnimationCurve animationCurve = outsideInfo.SpawnWeights.GetFor(ctx) ?? AnimationCurve.Constant(0, 1, 0);

        int randomNumberToSpawn;
        if (mapObjectInfo.HasNetworkObject)
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

        float objectWidth = outsideInfo.SpawnableOutsideObject.objectWidth;

        int attemptsDone = 0;
        int initialRandomNumberToSpawn = randomNumberToSpawn;
        while (randomNumberToSpawn > 0 && attemptsDone < 10 * initialRandomNumberToSpawn)
        {
            Debuggers.MapObjects?.Log($"Attempt number: {attemptsDone} with {randomNumberToSpawn} left to spawn with initial number: {initialRandomNumberToSpawn}.");
            attemptsDone++;
            System.Random rng = mapObjectInfo.HasNetworkObject ? serverOnlyRandom : everyoneRandom;

            GameObject? node = RoundManager.Instance.outsideAINodes[rng.Next(0, RoundManager.Instance.outsideAINodes.Length)];
            if (node == null)
            {
                DawnPlugin.Logger.LogWarning($"Failed to get a valid outside AI node to spawn map object at level: {level.sceneName}.");
                continue;
            }

            Vector3 spawnPos = node.transform.position;
            spawnPos = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(spawnPos, 10f, default, rng, -1) + (Vector3.up * 2f);

            if (!Physics.Raycast(spawnPos, Vector3.down, out RaycastHit hit, 100f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
                continue;

            if (!hit.collider)
                continue;

            string[] floorTags = outsideInfo.SpawnableOutsideObject.spawnableFloorTags;
            if (floorTags != null && floorTags.Length > 0)
            {
                bool validFloor = false;
                Transform hitTransform = hit.collider.transform;

                int footstepSurfaceIndex = -1;
                if (hitTransform.TryGetComponent(out DawnSurface surface) && (surface.SurfaceIndex > -1 || surface.TerrainIndices.Count > 0))
                {
                    surface.TryGetFootstepIndex(hit.point, false, out footstepSurfaceIndex);
                }

                for (int t = 0; t < floorTags.Length; t++)
                {
                    if (footstepSurfaceIndex != -1)
                    {
                        DawnSurfaceInfo surfaceInfo = StartOfRound.Instance.footstepSurfaces[footstepSurfaceIndex].GetDawnInfo();
                        if (surfaceInfo.Surface.surfaceTag.Equals(floorTags[t], StringComparison.OrdinalIgnoreCase))
                        {
                            validFloor = true;
                            break;
                        }
                    }
                    else if (hitTransform.CompareTag(floorTags[t]))
                    {
                        validFloor = true;
                        break;
                    }
                }

                if (!validFloor)
                    continue;
            }

            Vector3 finalPos = RoundManager.Instance.PositionEdgeCheck(hit.point, objectWidth);
            if (finalPos == Vector3.zero)
                continue;

            bool blocked = false;

            if (shipSpawnPathPoints != null)
            {
                for (int s = 0; s < shipSpawnPathPoints.Length; s++)
                {
                    if (Vector3.Distance(shipSpawnPathPoints[s].transform.position, finalPos) < objectWidth + 6)
                    {
                        blocked = true;
                        break;
                    }
                }
            }

            foreach (EntranceTeleport entranceTeleport in entranceTeleports)
            {
                if (Vector3.Distance(entranceTeleport.transform.position, finalPos) < objectWidth + 6)
                {
                    blocked = true;
                    break;
                }
            }

            if (blocked)
                continue;

            if (spawnDenialPoints != null)
            {
                for (int d = 0; d < spawnDenialPoints.Length; d++)
                {
                    if (Vector3.Distance(spawnDenialPoints[d].transform.position, finalPos) < objectWidth)
                    {
                        blocked = true;
                        break;
                    }
                }
            }

            if (blocked)
                continue;

            if (itemShipLandingNode != null && Vector3.Distance(itemShipLandingNode.transform.position, finalPos) < objectWidth)
            {
                continue;
            }

            if (objectWidth > 4f)
            {
                for (int p = 0; p < occupiedPositions.Count; p++)
                {
                    if (Vector3.Distance(finalPos, occupiedPositions[p].position) < objectWidth)
                    {
                        blocked = true;
                        break;
                    }
                }

                if (blocked)
                    continue;
            }

            randomNumberToSpawn--;

            occupiedPositions.Add((outsideInfo, finalPos));

            GameObject spawnedPrefab = GameObject.Instantiate(prefabToSpawn, finalPos, Quaternion.identity, RoundManager.Instance.mapPropsContainer.transform);

            Debuggers.MapObjects?.Log($"Spawning {spawnedPrefab.name} at {finalPos}");

            if (outsideInfo.AlignWithTerrain)
            {
                spawnedPrefab.transform.up = hit.normal;
            }

            Vector3 euler = spawnedPrefab.transform.eulerAngles;

            if (outsideInfo.SpawnableOutsideObject.spawnFacingAwayFromWall)
            {
                float yRot = RoundManager.Instance.YRotationThatFacesTheFarthestFromPosition(finalPos + Vector3.up * 0.2f, 25f, 6);
                euler.y = yRot;
            }
            else
            {
                int randomY = rng.Next(0, 360);
                euler.y = randomY;
            }

            spawnedPrefab.transform.eulerAngles = euler;
            spawnedPrefab.transform.localEulerAngles += outsideInfo.SpawnableOutsideObject.rotationOffset;

            _spawnedObjects++;
            if (!mapObjectInfo.HasNetworkObject)
                continue;

            spawnedPrefab.GetComponent<NetworkObject>().Spawn(true);
        }

        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        List<GameObject> trees = GameObject.FindGameObjectsWithTag("Tree").ToList();
        for (int i = 0; i < occupiedPositions.Count; i++)
        {
            if (occupiedPositions[i].outsideMapObjectInfo.SpawnableOutsideObject.destroyTrees)
            {
                for (int j = trees.Count - 1; j >= 0; j--)
                {
                    float distanceTreeToOccupiedPosition = Vector3.Distance(trees[j].transform.position, occupiedPositions[i].position);
                    if (distanceTreeToOccupiedPosition < occupiedPositions[i].outsideMapObjectInfo.SpawnableOutsideObject.objectWidth)
                    {
                        RoundManager.Instance.DestroyTreeAtPosition(trees[j].transform.position);
                        trees.RemoveAt(j);
                    }
                }
            }
        }
    }

    private static void UpdateMapObjectSpawnWeights(On.StartOfRound.orig_SetPlanetsWeather orig, StartOfRound self, int connectedPlayersOnServer)
    {
        orig(self, connectedPlayersOnServer);
        UpdateIndoorMapHazardSpawnWeightsOnLevel(self.currentLevel);
        UpdateOutsideMapObjectSpawnWeightsOnLevel(self.currentLevel);
    }

    internal static void UpdateIndoorMapHazardSpawnWeightsOnLevel(SelectableLevel level)
    {
        if (!LethalContent.Weathers.IsFrozen || !LethalContent.MapObjects.IsFrozen || StartOfRound.Instance == null || (WeatherRegistryCompat.Enabled && !WeatherRegistryCompat.IsWeatherManagerReady()))
            return;

        foreach (DawnMapObjectInfo mapObjectInfo in LethalContent.MapObjects.Values)
        {
            DawnInsideMapObjectInfo? insideInfo = mapObjectInfo.InsideInfo;
            if (insideInfo == null || mapObjectInfo.ShouldSkipRespectOverride())
                continue;

            Debuggers.MapObjects?.Log($"Updating weights for {insideInfo.IndoorMapHazardType.prefabToSpawn.name} on level {level.PlanetName}");
            IndoorMapHazard? indoorMapHazard = level.indoorMapHazards.FirstOrDefault(mapObject => mapObject.hazardType == insideInfo.IndoorMapHazardType);
            if (indoorMapHazard == null)
            {
                indoorMapHazard = new()
                {
                    hazardType = insideInfo.IndoorMapHazardType,
                    numberToSpawn = AnimationCurve.Constant(0, 1, 0)
                };

                List<IndoorMapHazard> newIndoorMapHazard = level.indoorMapHazards.ToList();
                newIndoorMapHazard.Add(indoorMapHazard);
                level.indoorMapHazards = newIndoorMapHazard.ToArray();
            }

            SpawnWeightContext ctx = new SpawnWeightContext(
                level.GetDawnInfo(),
                RoundManager.Instance.dungeonGenerator?.Generator?.DungeonFlow?.GetDawnInfo(),
                TimeOfDayRefs.GetCurrentWeatherEffect(level)?.GetDawnInfo())
                .WithExtra(SpawnWeightExtraKeys.RoutingPriceKey, level.GetDawnInfo().DawnPurchaseInfo.Cost.Provide());

            indoorMapHazard.numberToSpawn = insideInfo.SpawnWeights.GetFor(ctx) ?? AnimationCurve.Constant(0, 1, 0);
        }
    }

    internal static void UpdateOutsideMapObjectSpawnWeightsOnLevel(SelectableLevel level)
    {
        if (!LethalContent.Weathers.IsFrozen || !LethalContent.MapObjects.IsFrozen || StartOfRound.Instance == null || (WeatherRegistryCompat.Enabled && !WeatherRegistryCompat.IsWeatherManagerReady()))
            return;

        foreach (DawnMapObjectInfo mapObjectInfo in LethalContent.MapObjects.Values)
        {
            DawnOutsideMapObjectInfo? outsideInfo = mapObjectInfo.OutsideInfo;
            if (outsideInfo == null || mapObjectInfo.ShouldSkipRespectOverride())
                continue;

            SpawnableOutsideObjectWithRarity? spawnableOutsideObjectWithRarity = level.spawnableOutsideObjects.FirstOrDefault(mapObject => mapObject.spawnableObject.prefabToSpawn == outsideInfo.SpawnableOutsideObject.prefabToSpawn);
            if (spawnableOutsideObjectWithRarity != null)
            {
                List<SpawnableOutsideObjectWithRarity> newSpawnableMapObjects = level.spawnableOutsideObjects.ToList();
                newSpawnableMapObjects.Remove(spawnableOutsideObjectWithRarity);
                level.spawnableOutsideObjects = newSpawnableMapObjects.ToArray();
                Debuggers.MapObjects?.Log($"Updating weights for {outsideInfo.SpawnableOutsideObject.prefabToSpawn.name} on level {level.PlanetName}");
            }
        }
    }

    private static void RegisterMapObjects()
    {
        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            List<IndoorMapHazard> newIndoorMapHazards = moonInfo.Level.indoorMapHazards.ToList();
            foreach (DawnMapObjectInfo mapObjectInfo in LethalContent.MapObjects.Values)
            {
                if (mapObjectInfo.InsideInfo == null || mapObjectInfo.ShouldSkipRespectOverride())
                    continue;

                IndoorMapHazard indoorMapHazard = new()
                {
                    hazardType = mapObjectInfo.InsideInfo.IndoorMapHazardType,
                    numberToSpawn = AnimationCurve.Constant(0, 1, 0)
                };

                newIndoorMapHazards.Add(indoorMapHazard);
            }

            moonInfo.Level.indoorMapHazards = newIndoorMapHazards.ToArray();
        }
        FreezeMapObjectContents();
    }
}