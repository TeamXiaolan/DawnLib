using System;

namespace Dawn.Dusk;

[Serializable]
public class CRMAdditionalTilesReference : CRMContentReference<CRMAdditionalTilesDefinition, DawnTileSetInfo>
{
    public CRMAdditionalTilesReference() : base()
    { }
    public CRMAdditionalTilesReference(NamespacedKey<DawnTileSetInfo> key) : base(key)
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