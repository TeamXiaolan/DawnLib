using System;
using Dawn;

namespace Dusk;

[Serializable]
public class DuskMapObjectReference : DuskContentReference<DuskMapObjectDefinition, DawnMapObjectInfo>
{
    public DuskMapObjectReference() : base()
    { }

    public DuskMapObjectReference(NamespacedKey<DawnMapObjectInfo> key) : base(key)
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