using System.Collections.Generic;
using DunGen;

namespace CodeRebirthLib;
public class TilesetInfoBuilder : BaseInfoBuilder<CRTileSetInfo, TileSet, TilesetInfoBuilder>
{
    private bool _branchCap, _regular = true;

    internal TilesetInfoBuilder(NamespacedKey<CRTileSetInfo> key, TileSet value) : base(key, value)
    {
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

    public TilesetInfoBuilder AddTag(NamespacedKey tag)
    {
        tags.Add(tag);
        return this;
    }

    override internal CRTileSetInfo Build()
    {
        return new CRTileSetInfo(key, tags, value, _branchCap, _regular);
    }
}