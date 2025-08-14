using System;

namespace CodeRebirthLib.CRMod;

[Serializable]
public class CRMMapObjectReference : CRMContentReference<CRMMapObjectDefinition, CRMapObjectInfo>
{
    public CRMMapObjectReference() : base()
    { }

    public CRMMapObjectReference(NamespacedKey<CRMapObjectInfo> key) : base(key)
    { }

    public override bool TryResolve(out CRMapObjectInfo info)
    {
        return LethalContent.MapObjects.TryGetValue(TypedKey, out info);
    }

    public override CRMapObjectInfo Resolve()
    {
        return LethalContent.MapObjects[TypedKey];
    }
}