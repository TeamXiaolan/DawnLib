using System.Collections.Generic;
using DunGen.Graph;
using System;
using System.Linq;
using DunGen;
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
        List<TileSet> tileSets = new();
        foreach (TileSet tileSet in DungeonFlow.GetUsedTileSets())
        {
            if (tileSet == null)
            {
                DawnPlugin.Logger.LogWarning("TileSet is null in dungeonflow: " + DungeonFlow.name);
                continue;
            }

            if (tileSets.Contains(tileSet))
            {
                continue;
            }

            tileSets.Add(tileSet);
        }

        foreach (TileInjectionRule tileInjectionRule in DungeonFlow.TileInjectionRules)
        {
            if (tileInjectionRule.TileSet == null)
            {
                DawnPlugin.Logger.LogWarning("TileSet is null in a tileInjectionRule");
                continue;
            }

            if (tileSets.Contains(tileInjectionRule.TileSet))
            {
                continue;
            }

            tileSets.Add(tileInjectionRule.TileSet);
        }

        foreach (TileSet tileSet in tileSets)
        {
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
                }
            }
        }

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