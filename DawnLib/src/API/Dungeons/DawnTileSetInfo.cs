using System;
using System.Collections.Generic;
using System.Linq;
using DunGen;
using Unity.Netcode;

namespace Dawn;

public sealed class DawnTileSetInfo : DawnBaseInfo<DawnTileSetInfo>
{
    internal List<DoorwaySocket> sockets = new();
    internal List<Doorway> doorways = new();
    internal List<SpawnSyncedObject> spawnSyncedObjects = new();
    internal List<Tile> tiles = new();

    internal DawnTileSetInfo(NamespacedKey<DawnTileSetInfo> key, HashSet<NamespacedKey> tags, IPredicate injectionRule, TileSet tileSet, bool isBranchCap, bool isRegular, IDataContainer? customData) : base(key, tags, customData)
    {
        TileSet = tileSet;
        IsBranchCap = isBranchCap;
        IsRegular = isRegular;
        InjectionPredicate = injectionRule;

        if (!ShouldSkipIgnoreOverride())
            return;

        SetupDetails();
        RegisterSpawnSyncedObjects();
    }

    internal void RegisterSpawnSyncedObjects()
    {
        if (TileSet == null)
        {
            DawnPlugin.Logger.LogWarning($"Trying to register spawnSyncedObjects for likely-dawnlib tileSet: {TypedKey} that isn't loaded");
            return;
        }

        foreach (SpawnSyncedObject spawnSyncedObject in SpawnSyncedObjects)
        {
            if (spawnSyncedObject.spawnPrefab == null)
            {
                DawnPlugin.Logger.LogWarning("SpawnSyncedObject.spawnPrefab is null in tileSet: " + TileSet.name);
                continue;
            }

            if (!spawnSyncedObject.spawnPrefab.TryGetComponent(out NetworkObject _))
            {
                continue;
            }

            DawnLib.RegisterNetworkPrefab(spawnSyncedObject.spawnPrefab);
        }
    }

    public TileSet? TileSet { get; internal set; } // Null if currently not hotloaded and is a dawnlib interior

    public bool IsBranchCap { get; }
    public bool IsRegular { get; }
    public IPredicate InjectionPredicate { get; }

    public IReadOnlyList<Tile> Tiles => tiles.AsReadOnly();
    public IReadOnlyList<Doorway> Doorways => doorways.AsReadOnly();
    public IReadOnlyList<SpawnSyncedObject> SpawnSyncedObjects => spawnSyncedObjects.AsReadOnly();
    public IReadOnlyList<DoorwaySocket> Sockets => sockets.AsReadOnly();

    internal void SetupDetails()
    {
        tiles.Clear();
        doorways.Clear();
        spawnSyncedObjects.Clear();
        sockets.Clear();

        if (TileSet == null)
        {
            DawnPlugin.Logger.LogWarning($"Trying to setup details for likely-dawnlib tileSet: {TypedKey} that isn't loaded");
            return;
        }

        foreach (GameObjectChance gameObjectChance in TileSet.TileWeights.Weights)
        {
            if (gameObjectChance.Value == null)
            {
                DawnPlugin.Logger.LogWarning("GameObject is null in tileSet: " + TileSet.name);
                continue;
            }

            foreach (Tile tile in gameObjectChance.Value.GetComponentsInChildren<Tile>())
            {
                if (tile == null)
                {
                    DawnPlugin.Logger.LogWarning("Tile is null in tileSet: " + TileSet.name);
                    continue;
                }

                if (tiles.Contains(tile))
                {
                    continue;
                }

                tiles.Add(tile);
                DawnLib.FixDoorwaySockets(gameObjectChance.Value);
            }
        }

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
                    if (dungeonDoorway.socket == null)
                    {
                        DawnPlugin.Logger.LogWarning($"TileSet: {TileSet.name} has a null socket in doorway: {dungeonDoorway.name} from Tile: {dungeonTile.name}");
                        continue;
                    }

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

    internal static List<SpawnSyncedObject> GrabAllSpawnSyncedObjects(TileSet tileSet)
    {
        List<Tile> tiles = new();
        foreach (GameObjectChance gameObjectChance in tileSet.TileWeights.Weights)
        {
            if (gameObjectChance.Value == null)
            {
                DawnPlugin.Logger.LogWarning("GameObject is null in tileSet: " + tileSet.name);
                continue;
            }

            foreach (Tile tile in gameObjectChance.Value.GetComponentsInChildren<Tile>())
            {
                if (tile == null)
                {
                    DawnPlugin.Logger.LogWarning("Tile is null in tileSet: " + tileSet.name);
                    continue;
                }

                if (tiles.Contains(tile))
                {
                    continue;
                }

                tiles.Add(tile);
                DawnLib.FixDoorwaySockets(gameObjectChance.Value);
            }
        }

        List<SpawnSyncedObject> spawnSyncedObjects = new();
        foreach (Tile dungeonTile in tiles)
        {
            foreach (Doorway dungeonDoorway in dungeonTile.gameObject.GetComponentsInChildren<Doorway>())
            {
                foreach (GameObjectWeight doorwayTileWeight in dungeonDoorway.ConnectorPrefabWeights)
                {
                    foreach (SpawnSyncedObject spawnSyncedObject in doorwayTileWeight.GameObject.GetComponentsInChildren<SpawnSyncedObject>())
                    {
                        if (!spawnSyncedObjects.Contains(spawnSyncedObject))
                        {
                            spawnSyncedObjects.Add(spawnSyncedObject);
                        }
                    }
                }


                foreach (GameObjectWeight doorwayTileWeight in dungeonDoorway.BlockerPrefabWeights)
                {
                    foreach (SpawnSyncedObject spawnSyncedObject in doorwayTileWeight.GameObject.GetComponentsInChildren<SpawnSyncedObject>())
                    {
                        if (!spawnSyncedObjects.Contains(spawnSyncedObject))
                        {
                            spawnSyncedObjects.Add(spawnSyncedObject);
                        }
                    }
                }
            }

            foreach (SpawnSyncedObject spawnSyncedObject in dungeonTile.gameObject.GetComponentsInChildren<SpawnSyncedObject>())
            {
                if (!spawnSyncedObjects.Contains(spawnSyncedObject))
                {
                    spawnSyncedObjects.Add(spawnSyncedObject);
                }
            }
        }

        return spawnSyncedObjects;
    }
}