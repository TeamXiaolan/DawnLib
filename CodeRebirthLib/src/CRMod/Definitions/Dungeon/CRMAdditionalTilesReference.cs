using System;

namespace CodeRebirthLib.CRMod;

[Serializable]
public class CRMAdditionalTilesReference : CRMContentReference<CRMAdditionalTilesDefinition, CRTileSetInfo>
{
    public CRMAdditionalTilesReference() : base()
    { }
    public CRMAdditionalTilesReference(NamespacedKey<CRTileSetInfo> key) : base(key)
    { }

    public override bool TryResolve(out CRTileSetInfo info)
    {
        return LethalContent.TileSets.TryGetValue(TypedKey, out info);
    }

    public override CRTileSetInfo Resolve()
    {
        return LethalContent.TileSets[TypedKey];
    }
}