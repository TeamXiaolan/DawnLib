using System;
using System.Collections.Generic;
using System.Linq;
using DunGen;
using DunGen.Graph;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

namespace Dawn;

public class DawnDungeonInfo : DawnBaseInfo<DawnDungeonInfo>
{
    private List<DoorwaySocket> _sockets;

    internal DawnDungeonInfo(NamespacedKey<DawnDungeonInfo> key, HashSet<NamespacedKey> tags, DungeonFlow dungeonFlow, ProviderTable<int?, DawnMoonInfo>? weights, float mapTileSize, AudioClip? firstTimeAudio, IDataContainer? customData) : base(key, tags, customData)
    {
        DungeonFlow = dungeonFlow;
        Weights = weights;
        MapTileSize = mapTileSize;
        firstTimeAudio = FirstTimeAudio;
        _sockets = new();
        foreach (GameObjectChance chance in DungeonFlow.GetUsedTileSets().Select(it => it.TileWeights.Weights).SelectMany(it => it))
        {
            Doorway[] doorways = chance.Value.GetComponentsInChildren<Doorway>();

            foreach (Doorway doorway in doorways)
            {
                if (Sockets.Contains(doorway.socket))
                    continue;

                _sockets.Add(doorway.socket);
            }
        }
    }

    public DungeonFlow DungeonFlow { get; }
    public ProviderTable<int?, DawnMoonInfo>? Weights { get; }
    public float MapTileSize { get; }
    public AudioClip? FirstTimeAudio { get; }
    public IReadOnlyList<DoorwaySocket> Sockets => _sockets.AsReadOnly();
}