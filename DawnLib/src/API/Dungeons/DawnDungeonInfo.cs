using System.Collections.Generic;
using DunGen.Graph;
using UnityEngine;
using System;
using System.Linq;
using DunGen;
using UnityEngine.InputSystem.Utilities;
using Dawn.Utils;

namespace Dawn;

public class DawnDungeonInfo : DawnBaseInfo<DawnDungeonInfo>
{
    internal List<DoorwaySocket> sockets = new();
    internal List<Doorway> doorways = new();
    internal List<SpawnSyncedObject> spawnSyncedObjects = new();
    internal List<Tile> tiles = new();

    internal DawnDungeonInfo(NamespacedKey<DawnDungeonInfo> key, HashSet<NamespacedKey> tags, DungeonFlow dungeonFlow, ProviderTable<int?, DawnMoonInfo, SpawnWeightContext> weights, float mapTileSize, DawnStingerDetail stingerDetail, string assetBundlePath, BoundedRange dungeonClampRange, int extraScrapGeneration, IDataContainer? customData) : base(key, tags, customData)
    {
        DungeonFlow = dungeonFlow;
        Weights = weights;
        MapTileSize = mapTileSize;
        StingerDetail = stingerDetail;
        AssetBundlePath = assetBundlePath;
        DungeonClampRange = dungeonClampRange;
        ExtraScrapGeneration = extraScrapGeneration;

        if (!ShouldSkipIgnoreOverride())
            return;

        sockets = new();
        var tileSets = DungeonFlow.TileInjectionRules.Select(it => it.TileSet).Concat(DungeonFlow.GetUsedTileSets()).Distinct();
        tiles = tileSets.Select(it => it.TileWeights.Weights).SelectMany(it => it).SelectMany(it => it.Value.GetComponentsInChildren<Tile>()).ToList();
        doorways = new();
        spawnSyncedObjects = new();

        foreach (Tile dungeonTile in Tiles)
        {
            foreach (Doorway dungeonDoorway in dungeonTile.gameObject.GetComponentsInChildren<Doorway>())
            {
                if (!Doorways.Contains(dungeonDoorway))
                {
                    doorways.Add(dungeonDoorway);
                }

                if (!Sockets.Contains(dungeonDoorway.socket))
                {
                    sockets.Add(dungeonDoorway.socket);
                }

                foreach (GameObjectWeight doorwayTileWeight in dungeonDoorway.ConnectorPrefabWeights)
                {
                    foreach (SpawnSyncedObject spawnSyncedObject in doorwayTileWeight.GameObject.GetComponentsInChildren<SpawnSyncedObject>())
                    {
                        if (!SpawnSyncedObjects.Contains(spawnSyncedObject))
                        {
                            spawnSyncedObjects.Add(spawnSyncedObject);
                        }
                    }
                }


                foreach (GameObjectWeight doorwayTileWeight in dungeonDoorway.BlockerPrefabWeights)
                {
                    foreach (SpawnSyncedObject spawnSyncedObject in doorwayTileWeight.GameObject.GetComponentsInChildren<SpawnSyncedObject>())
                    {
                        if (!SpawnSyncedObjects.Contains(spawnSyncedObject))
                        {
                            spawnSyncedObjects.Add(spawnSyncedObject);
                        }
                    }
                }
            }

            foreach (SpawnSyncedObject spawnSyncedObject in dungeonTile.gameObject.GetComponentsInChildren<SpawnSyncedObject>())
            {
                if (!SpawnSyncedObjects.Contains(spawnSyncedObject))
                {
                    spawnSyncedObjects.Add(spawnSyncedObject);
                }
            }
        }
    }

    public DungeonFlow DungeonFlow { get; }
    public string AssetBundlePath { get; }
    public ProviderTable<int?, DawnMoonInfo, SpawnWeightContext> Weights { get; private set; }
    public float MapTileSize { get; private set; }
    public DawnStingerDetail StingerDetail { get; private set; }
    public BoundedRange DungeonClampRange { get; private set; }
    public int ExtraScrapGeneration { get; private set; }

    public IReadOnlyList<Tile> Tiles => tiles.AsReadOnly();
    public IReadOnlyList<Doorway> Doorways => doorways.AsReadOnly();
    public IReadOnlyList<SpawnSyncedObject> SpawnSyncedObjects => spawnSyncedObjects.AsReadOnly();
    public IReadOnlyList<DoorwaySocket> Sockets => sockets.AsReadOnly();

    public static int FireExitGlobalPropID = 1231;
}