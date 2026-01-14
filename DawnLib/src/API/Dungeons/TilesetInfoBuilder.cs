using DunGen;

namespace Dawn;
public class TilesetInfoBuilder : BaseInfoBuilder<DawnTileSetInfo, TileSet, TilesetInfoBuilder>
{
    private bool _branchCap, _regular = true;
    private IPredicate? _predicate;

    internal TilesetInfoBuilder(NamespacedKey<DawnTileSetInfo> key, TileSet value) : base(key, value)
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

    public TilesetInfoBuilder SetInjectionPredicate(IPredicate predicate)
    {
        _predicate = predicate;
        return this;
    }

    override internal DawnTileSetInfo Build()
    {
        _predicate ??= ConstantPredicate.True;

        // tilesets do not really need tags, its just there to carry the IsExternal flag
        return new DawnTileSetInfo(key, [], _predicate, value, _branchCap, _regular, customData);
    }
}