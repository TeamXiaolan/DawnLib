using System.Collections.Generic;
using DunGen;

namespace CodeRebirthLib.Dungeons;
public class CRTileSetInfo : INamespaced<CRTileSetInfo>
{
    internal CRTileSetInfo(NamespacedKey<CRTileSetInfo> key, TileSet tileSet, IReadOnlyList<NamespacedKey<CRDungeonInfo>> appliedTo, bool isBranchCap, bool isRegular)
    {
        AppliedTo = appliedTo;
        TypedKey = key;
        TileSet = tileSet;
        IsBranchCap = isBranchCap;
        IsRegular = isRegular;
    }

    public TileSet TileSet { get; }
    public NamespacedKey Key => TypedKey;
    public NamespacedKey<CRTileSetInfo> TypedKey { get; }
    public IReadOnlyList<NamespacedKey<CRDungeonInfo>> AppliedTo { get; }
    
    public bool IsBranchCap { get; }
    public bool IsRegular { get; }
}