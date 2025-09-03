using System;

namespace Dawn.Dusk;

[Serializable]
public class CRMMapObjectReference : CRMContentReference<CRMMapObjectDefinition, DawnMapObjectInfo>
{
    public CRMMapObjectReference() : base()
    { }

    public CRMMapObjectReference(NamespacedKey<DawnMapObjectInfo> key) : base(key)
    { }

    public override bool TryResolve(out DawnMapObjectInfo info)
    {
        return LethalContent.MapObjects.TryGetValue(TypedKey, out info);
    }

    public override DawnMapObjectInfo Resolve()
    {
        return LethalContent.MapObjects[TypedKey];
    }
}