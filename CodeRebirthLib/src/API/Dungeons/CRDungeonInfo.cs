using System.Collections.Generic;
using DunGen.Graph;

namespace CodeRebirthLib;
public class CRDungeonInfo : CRBaseInfo<CRDungeonInfo>
{
    internal CRDungeonInfo(NamespacedKey<CRDungeonInfo> key, List<NamespacedKey> tags, DungeonFlow dungeonFlow) : base(key, tags)
    {
        DungeonFlow = dungeonFlow;
    }

    public DungeonFlow DungeonFlow { get; }
}