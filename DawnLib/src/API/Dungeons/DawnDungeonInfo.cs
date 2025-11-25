using System.Collections.Generic;
using DunGen.Graph;
using UnityEngine;
using System;
using System.Linq;
using DunGen;
using UnityEngine.InputSystem.Utilities;

namespace Dawn;

public class DawnDungeonInfo : DawnBaseInfo<DawnDungeonInfo>
{
    private List<DoorwaySocket> _sockets;
    private List<Doorway> _doorways;
    private List<SpawnSyncedObject> _spawnSyncedObjects;
    private List<Tile> _tiles;

    internal DawnDungeonInfo(NamespacedKey<DawnDungeonInfo> key, HashSet<NamespacedKey> tags, DungeonFlow dungeonFlow, ProviderTable<int?, DawnMoonInfo> weights, float mapTileSize, AudioClip? firstTimeAudio, IDataContainer? customData) : base(key, tags, customData)
    {
        DungeonFlow = dungeonFlow;
        Weights = weights;
        MapTileSize = mapTileSize;
        FirstTimeAudio = firstTimeAudio;

        if (!ShouldSkipIgnoreOverride())
            return;

        _sockets = new();
        _tiles = DungeonFlow.GetUsedTileSets().Select(it => it.TileWeights.Weights).SelectMany(it => it).SelectMany(it => it.Value.GetComponentsInChildren<Tile>()).ToList();
        _doorways = new();
        _spawnSyncedObjects = new();

        foreach (Tile dungeonTile in Tiles)
        {
            foreach (Doorway dungeonDoorway in dungeonTile.gameObject.GetComponentsInChildren<Doorway>())
            {
                if (!_doorways.Contains(dungeonDoorway))
                {
                    _doorways.Add(dungeonDoorway);
                }

                if (!Sockets.Contains(dungeonDoorway.socket))
                {
                    _sockets.Add(dungeonDoorway.socket);
                }

                foreach (GameObjectWeight doorwayTileWeight in dungeonDoorway.ConnectorPrefabWeights)
                {
                    foreach (SpawnSyncedObject spawnSyncedObject in doorwayTileWeight.GameObject.GetComponentsInChildren<SpawnSyncedObject>())
                    {
                        if (!SpawnSyncedObjects.Contains(spawnSyncedObject))
                        {
                            _spawnSyncedObjects.Add(spawnSyncedObject);
                        }
                    }
                }


                foreach (GameObjectWeight doorwayTileWeight in dungeonDoorway.BlockerPrefabWeights)
                {
                    foreach (SpawnSyncedObject spawnSyncedObject in doorwayTileWeight.GameObject.GetComponentsInChildren<SpawnSyncedObject>())
                    {
                        if (!SpawnSyncedObjects.Contains(spawnSyncedObject))
                        {
                            _spawnSyncedObjects.Add(spawnSyncedObject);
                        }
                    }
                }
            }

            foreach (SpawnSyncedObject spawnSyncedObject in dungeonTile.gameObject.GetComponentsInChildren<SpawnSyncedObject>())
            {
                if (!SpawnSyncedObjects.Contains(spawnSyncedObject))
                {
                    _spawnSyncedObjects.Add(spawnSyncedObject);
                }
            }
        }
    }

    public DungeonFlow DungeonFlow { get; }
    public string AssetBundlePath { get; }
    public ProviderTable<int?, DawnMoonInfo> Weights { get; private set; }
    public float MapTileSize { get; private set; }
    public AudioClip? FirstTimeAudio { get; }

    public IReadOnlyList<Tile> Tiles => _tiles.AsReadOnly();
    public IReadOnlyList<Doorway> Doorways => _doorways.AsReadOnly();
    public IReadOnlyList<SpawnSyncedObject> SpawnSyncedObjects => _spawnSyncedObjects.AsReadOnly();
    public IReadOnlyList<DoorwaySocket> Sockets => _sockets.AsReadOnly();

    public static int FireExitGlobalPropID = 1231;
}