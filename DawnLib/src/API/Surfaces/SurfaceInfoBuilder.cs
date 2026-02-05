using UnityEngine;

namespace Dawn;

public class SurfaceInfoBuilder : BaseInfoBuilder<DawnSurfaceInfo, FootstepSurface, SurfaceInfoBuilder>
{
    private GameObject? _surfaceVFXPrefab = null;
    private Vector3 _surfaceVFXOffset = Vector3.zero;

    internal SurfaceInfoBuilder(NamespacedKey<DawnSurfaceInfo> key, FootstepSurface value) : base(key, value)
    {
    }

    public SurfaceInfoBuilder SetSurfaceVFXPrefab(GameObject? surfaceVFXPrefab)
    {
        _surfaceVFXPrefab = surfaceVFXPrefab;
        return this;
    }

    public SurfaceInfoBuilder OverrideSurfaceVFXOffset(Vector3 surfaceVFXOffset)
    {
        _surfaceVFXOffset = surfaceVFXOffset;
        return this;
    }

    override internal DawnSurfaceInfo Build()
    {
        value.surfaceTag = "AnomalyObject";
        return new DawnSurfaceInfo(key, [], value, _surfaceVFXPrefab, _surfaceVFXOffset, -1, customData);
    }
}