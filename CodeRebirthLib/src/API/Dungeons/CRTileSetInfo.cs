using System.Collections.Generic;
using DunGen;

namespace CodeRebirthLib;
public class CRTileSetInfo : CRBaseInfo<CRTileSetInfo>
{
    internal CRTileSetInfo(NamespacedKey<CRTileSetInfo> key, List<NamespacedKey> tags, TileSet tileSet, bool isBranchCap, bool isRegular) : base(key, tags)
    {
        TileSet = tileSet;
        IsBranchCap = isBranchCap;
        IsRegular = isRegular;
    }

    public TileSet TileSet { get; }

    public bool IsBranchCap { get; }
    public bool IsRegular { get; }
}