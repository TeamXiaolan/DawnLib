using System;

namespace CodeRebirthLib.CRMod;

[Serializable]
public class CRMapObjectReference : CRContentReference<CRItemDefinition, CRMapObjectInfo>
{
    public CRMapObjectReference() : base()
    { }
    public CRMapObjectReference(NamespacedKey<CRMapObjectInfo> key) : base(key)
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