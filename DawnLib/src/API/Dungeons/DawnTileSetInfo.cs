using System.Collections.Generic;
using DunGen;

namespace Dawn;
public class DawnTileSetInfo : DawnBaseInfo<DawnTileSetInfo>
{

    internal DawnTileSetInfo(NamespacedKey<DawnTileSetInfo> key, HashSet<NamespacedKey> tags, IPredicate injectionRule, TileSet tileSet, bool isBranchCap, bool isRegular, IDataContainer? customData) : base(key, tags, customData)
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