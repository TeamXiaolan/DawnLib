using System;
using Dawn;

namespace Dusk;

[Serializable]
public class DuskAdditionalTilesReference : DuskContentReference<DuskAdditionalTilesDefinition, DawnTileSetInfo>
{
    public DuskAdditionalTilesReference() : base()
    { }
    public DuskAdditionalTilesReference(NamespacedKey<DawnTileSetInfo> key) : base(key)
    { }

    public override bool TryResolve(out DawnTileSetInfo info)
    {
        return LethalContent.TileSets.TryGetValue(TypedKey, out info);
    }

    public override DawnTileSetInfo Resolve()
    {
        return LethalContent.TileSets[TypedKey];
    }
}