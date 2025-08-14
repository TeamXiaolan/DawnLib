using System.Collections.Generic;
using DunGen;

namespace CodeRebirthLib;
public class TilesetInfoBuilder : BaseInfoBuilder<CRTileSetInfo, TileSet>
{
    private List<NamespacedKey<CRDungeonInfo>> _appliedTo = [];
    private bool _branchCap, _regular = true;

    internal TilesetInfoBuilder(NamespacedKey<CRTileSetInfo> key, TileSet value) : base(key, value)
    {
    }

    public TilesetInfoBuilder AddToDungeon(NamespacedKey<CRDungeonInfo> dungeonKey)
    {
        _appliedTo.Add(dungeonKey);
        return this;
    }

    public TilesetInfoBuilder SetIsBranchCap(bool value)
    {
        _branchCap = value;
        return this;
    }

    public TilesetInfoBuilder SetIsRegular(bool value)
    {
        _regular = value;
        return this;
    }

    override internal CRTileSetInfo Build()
    {
        return new CRTileSetInfo(_key, false, _value, _appliedTo, _branchCap, _regular);
    }
}