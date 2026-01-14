using System.Collections.Generic;
using UnityEngine;

namespace Dawn;

public sealed class DawnSurfaceInfo : DawnBaseInfo<DawnSurfaceInfo>
{
    internal DawnSurfaceInfo(NamespacedKey<DawnSurfaceInfo> key, HashSet<NamespacedKey> tags, FootstepSurface surface, GameObject? surfaceVFXPrefab, Vector3 surfaceVFXOffset, int surfaceIndex, IDataContainer? customData) : base(key, tags, customData)
    {
        Surface = surface;
        SurfaceVFXPrefab = surfaceVFXPrefab;
        SurfaceVFXOffset = surfaceVFXOffset;
        SurfaceIndex = surfaceIndex;
    }

    public FootstepSurface Surface { get; internal set; }
    public GameObject? SurfaceVFXPrefab { get; set; }
    public Vector3 SurfaceVFXOffset { get; set; }

    public int SurfaceIndex { get; internal set; } = -1;
}