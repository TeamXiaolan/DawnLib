using System.Collections.Generic;
using System.Linq;
using DunGen;
using DunGen.Graph;

namespace CodeRebirthLib;

public class CRDungeonInfo : CRBaseInfo<CRDungeonInfo>
{
    internal CRDungeonInfo(NamespacedKey<CRDungeonInfo> key, List<NamespacedKey> tags, DungeonFlow dungeonFlow) : base(key, tags)
    {
        DungeonFlow = dungeonFlow;
        Sockets = new();
        foreach (GameObjectChance chance in DungeonFlow.GetUsedTileSets().Select(it => it.TileWeights.Weights).SelectMany(it => it))
        {
            Doorway[] doorways = chance.Value.GetComponentsInChildren<Doorway>();

            foreach (Doorway doorway in doorways)
            {
                if (Sockets.Contains(doorway.socket))
                    continue;

                Sockets.Add(doorway.socket);
            }
        }
    }

    public DungeonFlow DungeonFlow { get; }
    public List<DoorwaySocket> Sockets { get; }
}