using System;
using System.Collections.Generic;
using System.Linq;
using DunGen;
using DunGen.Graph;
using UnityEngine.InputSystem.Utilities;

namespace CodeRebirthLib;

public class CRDungeonInfo : CRBaseInfo<CRDungeonInfo>
{
    private List<DoorwaySocket> _sockets;
    private List<CRTileSetInfo> _addedTilesets;
    
    internal CRDungeonInfo(NamespacedKey<CRDungeonInfo> key, List<NamespacedKey> tags, DungeonFlow dungeonFlow) : base(key, tags)
    {
        DungeonFlow = dungeonFlow;
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
    public IReadOnlyList<DoorwaySocket> Sockets => _sockets.AsReadOnly();
    public IReadOnlyList<CRTileSetInfo> AdditionalTileSets => _addedTilesets.AsReadOnly();
    
    public void AddTileSet(CRTileSetInfo info)
    {
        if (LethalContent.Dungeons.IsFrozen) throw new RegistryFrozenException();
        _addedTilesets.Add(info);
    }
}