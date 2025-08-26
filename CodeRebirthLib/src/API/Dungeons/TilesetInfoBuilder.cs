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


    override internal CRTileSetInfo Build()
    {
        // tilesets do not really need tags, its just there to carry the IsExternal flag
        return new CRTileSetInfo(key, [], value, _branchCap, _regular);
    }
}