using System.Collections.Generic;
using DunGen;

namespace CodeRebirthLib;
public class TilesetInfoBuilder : BaseInfoBuilder<CRTileSetInfo, TileSet>
{
    private bool _branchCap, _regular = true;
    private List<NamespacedKey> _tags = new();

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
        _tags.Add(tag);
        return this;
    }

    override internal CRTileSetInfo Build()
    {
        return new CRTileSetInfo(_key, _tags, _value, _branchCap, _regular);
    }
}