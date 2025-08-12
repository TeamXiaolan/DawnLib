using System;

namespace CodeRebirthLib.CRMod;

[Serializable]
public class CRAdditionalTilesReference : CRContentReference<CRAdditionalTilesDefinition, CRTileSetInfo>
{
    public CRAdditionalTilesReference() : base()
    { }
    public CRAdditionalTilesReference(NamespacedKey<CRTileSetInfo> key) : base(key)
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