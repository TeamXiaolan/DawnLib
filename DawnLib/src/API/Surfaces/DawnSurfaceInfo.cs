using System.Collections.Generic;
using UnityEngine;

namespace Dawn;

public sealed class DawnSurfaceInfo : DawnBaseInfo<DawnSurfaceInfo>
{
    internal DawnSurfaceInfo(NamespacedKey<DawnSurfaceInfo> key, HashSet<NamespacedKey> tags, FootstepSurface surface, bool isNatural, bool quicksandCompatible, GameObject? surfaceVFXPrefab, Vector3 surfaceVFXOffset, int surfaceIndex, IDataContainer? customData) : base(key, tags, customData)
    {
        Surface = surface;
        IsNatural = isNatural;
        QuicksandCompatible = quicksandCompatible;
        SurfaceVFXPrefab = surfaceVFXPrefab;
        SurfaceVFXOffset = surfaceVFXOffset;
        SurfaceIndex = surfaceIndex;
    }

    public FootstepSurface Surface { get; internal set; }
    public bool IsNatural { get; }
    public bool QuicksandCompatible { get; }
    public GameObject? SurfaceVFXPrefab { get; }
    public Vector3 SurfaceVFXOffset { get; }

    public int SurfaceIndex { get; internal set; } = -1;
}