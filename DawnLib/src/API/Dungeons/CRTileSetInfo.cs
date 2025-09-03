using System.Collections.Generic;
using DunGen;

namespace Dawn;
public class CRTileSetInfo : CRBaseInfo<CRTileSetInfo>
{
    
    internal CRTileSetInfo(NamespacedKey<CRTileSetInfo> key, List<NamespacedKey> tags, IPredicate injectionRule, TileSet tileSet, bool isBranchCap, bool isRegular) : base(key, tags)
    {
        TileSet = tileSet;
        IsBranchCap = isBranchCap;
        IsRegular = isRegular;
        InjectionPredicate = injectionRule;
    }

    public TileSet TileSet { get; }

    public bool IsBranchCap { get; }
    public bool IsRegular { get; }
    public IPredicate InjectionPredicate { get; }
}