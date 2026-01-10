using System;
using Dawn;

namespace Dusk;

[Serializable]
public class DuskSurfaceReference : DuskContentReference<DuskSurfaceDefinition, DawnSurfaceInfo>
{
    public DuskSurfaceReference() : base()
    { }
    public DuskSurfaceReference(NamespacedKey<DawnSurfaceInfo> key) : base(key)
    { }
    public override bool TryResolve(out DawnSurfaceInfo info)
    {
        return LethalContent.Surfaces.TryGetValue(TypedKey, out info);
    }

    public override DawnSurfaceInfo Resolve()
    {
        return LethalContent.Surfaces[TypedKey];
    }
}