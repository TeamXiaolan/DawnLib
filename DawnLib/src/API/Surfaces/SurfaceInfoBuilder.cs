using UnityEngine;

namespace Dawn;

public class SurfaceInfoBuilder : BaseInfoBuilder<DawnSurfaceInfo, FootstepSurface, SurfaceInfoBuilder>
{
    private GameObject? _surfaceVFXPrefab = null;
    private Vector3 _surfaceVFXOffset = Vector3.zero;
    private bool _isNatural, quicksandCompatible = false;

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

    public SurfaceInfoBuilder OverrideIsNatural(bool isNatural)
    {
        _isNatural = isNatural;
        return this;
    }

    public SurfaceInfoBuilder OverrideQuicksandCompatible(bool quicksandCompatible)
    {
        this.quicksandCompatible = quicksandCompatible;
        return this;
    }

    override internal DawnSurfaceInfo Build()
    {
        value.surfaceTag = "AnomalyObject";
        return new DawnSurfaceInfo(key, [], value, _isNatural, quicksandCompatible, _surfaceVFXPrefab, _surfaceVFXOffset, -1, customData);
    }
}