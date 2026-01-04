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
    private static List<GameObject> _vanillaMapObjects = new();

    internal static void Init()
    {
        using (new DetourContext(priority: int.MaxValue))
        {
            On.StartOfRound.Awake += CollectVanillaMapObjects;
        }

        DawnPlugin.Hooks.Add(new Hook(AccessTools.DeclaredMethod(typeof(RandomMapObject), "Awake"), AddPrefabsToRandomMapObjects));

        On.RoundManager.SpawnOutsideHazards += SpawnOutsideMapObjects;
        IL.RoundManager.SpawnOutsideHazards += RegenerateNavMeshTranspiler;

        On.StartOfRound.SetPlanetsWeather += UpdateMapObjectSpawnWeights;
        On.RoundManager.SpawnMapObjects += UpdateMapObjectSpawnWeights;

        LethalContent.Moons.OnFreeze += RegisterMapObjects;
        LethalContent.MapObjects.OnFreeze += FixMapObjectBlanks;
    }

    private static void AddPrefabsToRandomMapObjects(RuntimeILReferenceBag.FastDelegateInvokers.Action<RandomMapObject> orig, RandomMapObject self)
    {
        foreach (DawnMapObjectInfo mapObjectInfo in LethalContent.MapObjects.Values)
        {
            if (mapObjectInfo.InsideInfo == null || mapObjectInfo.ShouldSkipRespectOverride() || !mapObjectInfo.HasNetworkObject)
                continue;

            self.spawnablePrefabs.Add(mapObjectInfo.MapObject);
        }
        orig(self);
    }

    private static void CollectVanillaMapObjects(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        if (LethalContent.MapObjects.IsFrozen)
        {
            orig(self);
            return;
        }

        foreach (SelectableLevel selectableLevel in self.levels)
        {
            Debuggers.MapObjects?.Log($"Collecting vanilla map objects from supposedly vanilla level {selectableLevel.name}");
            _vanillaMapObjects.AddRange(selectableLevel.spawnableMapObjects.Select(x => x.prefabToSpawn));
            _vanillaMapObjects.AddRange(selectableLevel.spawnableOutsideObjects.Select(x => x.spawnableObject.prefabToSpawn));
        }
        _vanillaMapObjects = _vanillaMapObjects.Distinct().ToList();
        orig(self);
    }

    private static void FixMapObjectBlanks()
    {
        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            if (moonInfo.ShouldSkipIgnoreOverride())
                continue;

            foreach (SpawnableMapObject spawnableMapObject in moonInfo.Level.spawnableMapObjects)
            {
                if (spawnableMapObject.prefabToSpawn == null)
                    continue;

                foreach (DawnMapObjectInfo mapObjectInfo in LethalContent.MapObjects.Values)
                {
                    if (mapObjectInfo.InsideInfo == null)
                        continue;

                    if (mapObjectInfo.MapObject.name != spawnableMapObject.prefabToSpawn.name)
                        continue;

                    spawnableMapObject.prefabToSpawn = mapObjectInfo.MapObject;
                    spawnableMapObject.spawnFacingAwayFromWall = mapObjectInfo.InsideInfo.SpawnFacingAwayFromWall;
                    spawnableMapObject.spawnFacingWall = mapObjectInfo.InsideInfo.SpawnFacingWall;
                    spawnableMapObject.spawnWithBackToWall = mapObjectInfo.InsideInfo.SpawnWithBackToWall;
                    spawnableMapObject.spawnWithBackFlushAgainstWall = mapObjectInfo.InsideInfo.SpawnWithBackFlushAgainstWall;
                    spawnableMapObject.requireDistanceBetweenSpawns = mapObjectInfo.InsideInfo.RequireDistanceBetweenSpawns;
                    spawnableMapObject.disallowSpawningNearEntrances = mapObjectInfo.InsideInfo.DisallowSpawningNearEntrances;
                    break;
                }
            }

            foreach (SpawnableOutsideObjectWithRarity spawnableOutsideObjectWithRarity in moonInfo.Level.spawnableOutsideObjects)
            {
                if (spawnableOutsideObjectWithRarity.spawnableObject?.prefabToSpawn == null)
                    continue;

                foreach (DawnMapObjectInfo mapObjectInfo in LethalContent.MapObjects.Values)
                {
                    if (mapObjectInfo.OutsideInfo == null)
                        continue;

                    if (mapObjectInfo.MapObject.name != spawnableOutsideObjectWithRarity.spawnableObject.prefabToSpawn.name)
                        continue;

                    spawnableOutsideObjectWithRarity.spawnableObject.prefabToSpawn = mapObjectInfo.MapObject;
                    spawnableOutsideObjectWithRarity.spawnableObject.spawnFacingAwayFromWall = mapObjectInfo.OutsideInfo.SpawnFacingAwayFromWall;
                    spawnableOutsideObjectWithRarity.spawnableObject.objectWidth = mapObjectInfo.OutsideInfo.ObjectWidth;
                    spawnableOutsideObjectWithRarity.spawnableObject.spawnableFloorTags = mapObjectInfo.OutsideInfo.SpawnableFloorTags;
                    spawnableOutsideObjectWithRarity.spawnableObject.rotationOffset = mapObjectInfo.OutsideInfo.RotationOffset;
                    spawnableOutsideObjectWithRarity.randomAmount = mapObjectInfo.OutsideInfo.SpawnWeights.GetFor(moonInfo) ?? AnimationCurve.Constant(0, 1, 0);
                    break;
                }
            }
        }
    }

    private static void RegenerateNavMeshTranspiler(ILContext il)
    {
        ILCursor c = new ILCursor(il);

        c.GotoNext(
            i => i.MatchLdloc(out _), // num2
            i => i.MatchLdcI4(0), // 0
            i => i.MatchBle(out _), // >

            // further matching so that it doesn't need to be updated with the game as much hopefully
            i => i.MatchLdstr("OutsideLevelNavMesh")
        );

        c.Index++;
        c.EmitDelegate((int spawned) => spawned + _spawnedObjects);
    }

    private static void FreezeMapObjectContents()
    {
        Dictionary<GameObject, CurveTableBuilder<DawnMoonInfo>> insideWeightsByPrefab = new();
        Dictionary<GameObject, CurveTableBuilder<DawnMoonInfo>> outsideWeightsByPrefab = new();

        Dictionary<string, InsideMapObjectSettings> insidePlacementByPrefab = new();
        Dictionary<string, OutsideMapObjectSettings> outsidePlacementByPrefab = new();

        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            SelectableLevel level = moonInfo.Level;
            foreach (SpawnableMapObject mapObject in level.spawnableMapObjects)
            {
                GameObject? prefab = mapObject.prefabToSpawn;
                if (prefab == null)
                    continue;

                foreach (GameObject gameObject in _vanillaMapObjects)
                {
                    if (gameObject.name == prefab.name)
                    {
                        prefab = gameObject;
                        break;
                    }
                }

                if (!insideWeightsByPrefab.TryGetValue(prefab, out CurveTableBuilder<DawnMoonInfo> builder))
                {
                    builder = new CurveTableBuilder<DawnMoonInfo>();
                    insideWeightsByPrefab[prefab] = builder;

                    if (!insidePlacementByPrefab.ContainsKey(prefab.name))
                    {
                        insidePlacementByPrefab[prefab.name] = new InsideMapObjectSettings()
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
                foreach (GameObject gameObject in _vanillaMapObjects)
                {
                    if (gameObject.name == prefab.name)
                    {
                        prefab = gameObject;
                        break;
                    }
                }

                if (!outsideWeightsByPrefab.TryGetValue(prefab, out CurveTableBuilder<DawnMoonInfo> builder))
                {
                    builder = new CurveTableBuilder<DawnMoonInfo>();
                    outsideWeightsByPrefab[prefab] = builder;

                    if (!outsidePlacementByPrefab.ContainsKey(prefab.name))
                    {
                        outsidePlacementByPrefab[prefab.name] = new OutsideMapObjectSettings()
                        {
                            AlignWithTerrain = false,
                            MinimumAINodeSpawnRequirement = 0,
                            ObjectWidth = spawnable.objectWidth,
                            SpawnFacingAwayFromWall = spawnable.spawnFacingAwayFromWall,
                            SpawnableFloorTags = spawnable.spawnableFloorTags,
                            RotationOffset = spawnable.rotationOffset
                        };
                    }
                }

                builder.AddCurve(moonInfo.TypedKey, outsideMapObject.randomAmount);
            }
        }

        Dictionary<GameObject, DawnInsideMapObjectInfo> realInsideMapObjectsDict = new();
        foreach (var kvp in insideWeightsByPrefab)
        {
            GameObject prefab = kvp.Key;
            ProviderTable<AnimationCurve?, DawnMoonInfo> table = kvp.Value.Build();
            insidePlacementByPrefab.TryGetValue(prefab.name, out InsideMapObjectSettings mapObjectSettings);
            DawnInsideMapObjectInfo insideInfo = new(
                table,
                mapObjectSettings.spawnFacingAwayFromWall,
                mapObjectSettings.spawnFacingWall,
                mapObjectSettings.spawnWithBackToWall,
                mapObjectSettings.spawnWithBackFlushAgainstWall,
                mapObjectSettings.requireDistanceBetweenSpawns,
                mapObjectSettings.disallowSpawningNearEntrances
            );

            realInsideMapObjectsDict[prefab] = insideInfo;
        }

        Dictionary<GameObject, DawnOutsideMapObjectInfo> realOutsideMapObjectsDict = new();
        foreach (var kvp in outsideWeightsByPrefab)
        {
            GameObject prefab = kvp.Key;
            ProviderTable<AnimationCurve?, DawnMoonInfo> table = kvp.Value.Build();
            outsidePlacementByPrefab.TryGetValue(prefab.name, out OutsideMapObjectSettings mapObjectSettings);
            DawnOutsideMapObjectInfo outsideInfo = new(
                table,
                mapObjectSettings.SpawnFacingAwayFromWall,
                mapObjectSettings.ObjectWidth + 6,
                mapObjectSettings.SpawnableFloorTags ?? [],
                mapObjectSettings.RotationOffset,
                mapObjectSettings.AlignWithTerrain,
                mapObjectSettings.MinimumAINodeSpawnRequirement
            );
            realOutsideMapObjectsDict[prefab] = outsideInfo;
        }

        List<GameObject> realMapObjects = insideWeightsByPrefab.Keys
            .Concat(outsideWeightsByPrefab.Keys)
            .Distinct()
            .ToList();

        foreach (GameObject mapObject in realMapObjects)
        {
            if (mapObject.GetComponent<DawnMapObjectNamespacedKeyContainer>())
            {
                Debuggers.MapObjects?.Log($"Already registered {mapObject}");
                continue;
            }

            string name = NamespacedKey.NormalizeStringForNamespacedKey(mapObject.name, true);
            NamespacedKey<DawnMapObjectInfo>? key = MapObjectKeys.GetByReflection(name);
            if (key == null && LethalLibCompat.Enabled && LethalLibCompat.TryGetMapObjectFromLethalLib(mapObject, out string lethalLibModName))
            {
                key = NamespacedKey<DawnMapObjectInfo>.From(NamespacedKey.NormalizeStringForNamespacedKey(lethalLibModName, false), NamespacedKey.NormalizeStringForNamespacedKey(mapObject.name, false));
            }
            else if (key == null)
            {
                key = NamespacedKey<DawnMapObjectInfo>.From("unknown_lib", NamespacedKey.NormalizeStringForNamespacedKey(mapObject.name, false));
            }

            if (LethalContent.MapObjects.ContainsKey(key))
            {
                DawnPlugin.Logger.LogWarning($"MapObject {mapObject.name} is already registered by the same creator to LethalContent. This is likely to cause issues.");
                DawnMapObjectNamespacedKeyContainer duplicateContainer = mapObject.AddComponent<DawnMapObjectNamespacedKeyContainer>();
                duplicateContainer.Value = key;
                continue;
            }

            realInsideMapObjectsDict.TryGetValue(mapObject, out DawnInsideMapObjectInfo? insideMapObjectInfo);
            realOutsideMapObjectsDict.TryGetValue(mapObject, out DawnOutsideMapObjectInfo? outsideMapObjectInfo);

            DawnMapObjectInfo mapObjectInfo = new(key, [DawnLibTags.IsExternal], mapObject, insideMapObjectInfo, outsideMapObjectInfo, null);
            DawnMapObjectNamespacedKeyContainer container = mapObject.AddComponent<DawnMapObjectNamespacedKeyContainer>();
            container.Value = key;
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
    {
        UpdateOutsideMapObjectSpawnWeightsOnLevel(self.currentLevel);

        System.Random everyoneRandom = new(StartOfRound.Instance.randomMapSeed + 69);
        System.Random serverOnlyRandom = new(StartOfRound.Instance.randomMapSeed + 6969);
        List<Vector3> occupiedPositions = new();
        EntranceTeleport[] entranceTeleports = GameObject.FindObjectsByType<EntranceTeleport>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
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
                DawnPlugin.Logger.LogError($"Failed to get outside parent info for {mapObjectInfo.MapObject.name}");
                continue;
            }

            HandleSpawningOutsideObjects(outsideInfo, occupiedPositions, everyoneRandom, serverOnlyRandom, entranceTeleports, shipSpawnPathPoints, spawnDenialPoints, itemShipLandingNode);
        }
        orig(self);
        _spawnedObjects = 0;
    }

    private static void HandleSpawningOutsideObjects(DawnOutsideMapObjectInfo outsideInfo, List<Vector3> occupiedPositions, System.Random everyoneRandom, System.Random serverOnlyRandom, EntranceTeleport[] entranceTeleports, Transform[] shipSpawnPathPoints, GameObject[] spawnDenialPoints, GameObject itemShipLandingNode)
    {
        if (RoundManager.Instance.outsideAINodes.Length <= outsideInfo.MinimumAINodeSpawnRequirement)
        {
            return;
        }

        SelectableLevel level = RoundManager.Instance.currentLevel;
        DawnMapObjectInfo mapObjectInfo = outsideInfo.ParentInfo;

        GameObject prefabToSpawn = mapObjectInfo.MapObject;
        AnimationCurve animationCurve = outsideInfo.SpawnWeights.GetFor(level.GetDawnInfo()) ?? AnimationCurve.Constant(0, 1, 0);

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

        float objectWidth = outsideInfo.ObjectWidth;

        for (int i = 0; i < randomNumberToSpawn; i++)
        {
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

            string[] floorTags = outsideInfo.SpawnableFloorTags;
            if (floorTags != null && floorTags.Length > 0)
            {
                bool validFloor = false;
                Transform hitTransform = hit.collider.transform;

                for (int t = 0; t < floorTags.Length; t++)
                {
                    if (hitTransform.CompareTag(floorTags[t]))
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
                    if (Vector3.Distance(finalPos, occupiedPositions[p]) < objectWidth)
                    {
                        blocked = true;
                        break;
                    }
                }

                if (blocked)
                    continue;
            }

            occupiedPositions.Add(finalPos);

            GameObject spawnedPrefab = Object.Instantiate(prefabToSpawn, finalPos, Quaternion.identity, RoundManager.Instance.mapPropsContainer.transform);

            Debuggers.MapObjects?.Log($"Spawning {spawnedPrefab.name} at {finalPos}");

            if (outsideInfo.AlignWithTerrain)
            {
                spawnedPrefab.transform.up = hit.normal;
            }

            Vector3 euler = spawnedPrefab.transform.eulerAngles;

            if (outsideInfo.SpawnFacingAwayFromWall)
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
            spawnedPrefab.transform.localEulerAngles += outsideInfo.RotationOffset;

            _spawnedObjects++;
            if (!mapObjectInfo.HasNetworkObject)
                continue;

            spawnedPrefab.GetComponent<NetworkObject>().Spawn(true);
        }
    }

    private static void UpdateMapObjectSpawnWeights(On.StartOfRound.orig_SetPlanetsWeather orig, StartOfRound self, int connectedPlayersOnServer)
    {
        orig(self, connectedPlayersOnServer);
        UpdateInsideMapObjectSpawnWeightsOnLevel(self.currentLevel);
        UpdateOutsideMapObjectSpawnWeightsOnLevel(self.currentLevel);
    }

    internal static void UpdateInsideMapObjectSpawnWeightsOnLevel(SelectableLevel level)
    {
        if (!LethalContent.Weathers.IsFrozen || !LethalContent.MapObjects.IsFrozen || StartOfRound.Instance == null || (WeatherRegistryCompat.Enabled && !WeatherRegistryCompat.IsWeatherManagerReady()))
            return;

        foreach (DawnMapObjectInfo mapObjectInfo in LethalContent.MapObjects.Values)
        {
            DawnInsideMapObjectInfo? insideInfo = mapObjectInfo.InsideInfo;
            if (insideInfo == null || mapObjectInfo.ShouldSkipRespectOverride())
                continue;

            Debuggers.MapObjects?.Log($"Updating weights for {mapObjectInfo.MapObject.name} on level {level.PlanetName}");
            SpawnableMapObject? spawnableMapObject = level.spawnableMapObjects.FirstOrDefault(mapObject => mapObject.prefabToSpawn == mapObjectInfo.MapObject);
            if (spawnableMapObject == null)
            {
                spawnableMapObject = new()
                {
                    prefabToSpawn = mapObjectInfo.MapObject,
                    spawnFacingAwayFromWall = insideInfo.SpawnFacingAwayFromWall,
                    spawnFacingWall = insideInfo.SpawnFacingWall,
                    spawnWithBackFlushAgainstWall = insideInfo.SpawnWithBackFlushAgainstWall,
                    spawnWithBackToWall = insideInfo.SpawnWithBackToWall,
                    requireDistanceBetweenSpawns = insideInfo.RequireDistanceBetweenSpawns,
                    disallowSpawningNearEntrances = insideInfo.DisallowSpawningNearEntrances,
                    numberToSpawn = AnimationCurve.Constant(0, 1, 0)
                };
                List<SpawnableMapObject> newSpawnableMapObjects = level.spawnableMapObjects.ToList();
                newSpawnableMapObjects.Add(spawnableMapObject);
                level.spawnableMapObjects = newSpawnableMapObjects.ToArray();
            }
            spawnableMapObject.numberToSpawn = insideInfo.SpawnWeights.GetFor(level.GetDawnInfo()) ?? AnimationCurve.Constant(0, 1, 0);
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

            SpawnableOutsideObjectWithRarity? spawnableOutsideObjectWithRarity = level.spawnableOutsideObjects.FirstOrDefault(mapObject => mapObject.spawnableObject.prefabToSpawn == mapObjectInfo.MapObject);
            if (spawnableOutsideObjectWithRarity != null)
            {
                var newSpawnableMapObjects = level.spawnableOutsideObjects.ToList();
                newSpawnableMapObjects.Remove(spawnableOutsideObjectWithRarity);
                level.spawnableOutsideObjects = newSpawnableMapObjects.ToArray();
                Debuggers.MapObjects?.Log($"Updating weights for {mapObjectInfo.MapObject.name} on level {level.PlanetName}");
            }
        }
    }

    private static void RegisterMapObjects()
    {
        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            List<SpawnableMapObject> newSpawnableMapObjects = moonInfo.Level.spawnableMapObjects.ToList();
            foreach (DawnMapObjectInfo mapObjectInfo in LethalContent.MapObjects.Values)
            {
                if (mapObjectInfo.InsideInfo == null || mapObjectInfo.ShouldSkipRespectOverride())
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