using System.Collections.Generic;

namespace Dawn;

public sealed class DawnSurfaceInfo : DawnBaseInfo<DawnSurfaceInfo>
{
    internal DawnSurfaceInfo(NamespacedKey<DawnSurfaceInfo> key, HashSet<NamespacedKey> tags, FootstepSurface surface, int surfaceIndex, IDataContainer? customData) : base(key, tags, customData)
    {
        Surface = surface;
        SurfaceIndex = surfaceIndex;
    }

    public FootstepSurface? Surface { get; internal set; }

    public int SurfaceIndex { get; internal set; } = -1;
}