using System;
using System.Collections.Generic;
using System.Linq;
using DunGen;
using DunGen.Graph;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

namespace Dawn;

public class DawnDungeonInfo : DawnBaseInfo<DawnDungeonInfo>
{
    private List<DoorwaySocket> _sockets;
    private List<Doorway> _doorways;
    private List<SpawnSyncedObject> _spawnSyncedObjects;
    private List<Tile> _tiles;

    internal DawnDungeonInfo(NamespacedKey<DawnDungeonInfo> key, HashSet<NamespacedKey> tags, DungeonFlow dungeonFlow, ProviderTable<int?, DawnMoonInfo>? weights, float mapTileSize, AudioClip? firstTimeAudio, IDataContainer? customData) : base(key, tags, customData)
    {
        DungeonFlow = dungeonFlow;
        Weights = weights;
        MapTileSize = mapTileSize;
        firstTimeAudio = FirstTimeAudio;

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

        if (Key.IsVanilla())
        {
            return;
        }

        foreach (SpawnSyncedObject spawnSyncedObject in SpawnSyncedObjects)
        {
            if (spawnSyncedObject.spawnPrefab.GetComponent<NetworkObject>() == null)
            {
                spawnSyncedObject.spawnPrefab.AddComponent<NetworkObject>();
                // todo: do something to the network object's ID so it doesnt mess up?
            }

            DawnLib.RegisterNetworkPrefab(spawnSyncedObject.spawnPrefab);
        }
    }

    public DungeonFlow DungeonFlow { get; }
    public ProviderTable<int?, DawnMoonInfo>? Weights { get; }
    public float MapTileSize { get; }
    public AudioClip? FirstTimeAudio { get; }

    public IReadOnlyList<Tile> Tiles => _tiles.AsReadOnly();
    public IReadOnlyList<Doorway> Doorways => _doorways.AsReadOnly();
    public IReadOnlyList<SpawnSyncedObject> SpawnSyncedObjects => _spawnSyncedObjects.AsReadOnly();
    public IReadOnlyList<DoorwaySocket> Sockets => _sockets.AsReadOnly();
}