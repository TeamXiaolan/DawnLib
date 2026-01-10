using UnityEngine;

namespace Dawn;

public class SurfaceInfoBuilder : BaseInfoBuilder<DawnSurfaceInfo, FootstepSurface, SurfaceInfoBuilder>
{
    /* private string _surfaceTag;
    private AudioClip[]? _surfaceClips = null;
    private AudioClip? _hitSurfaceSFX = null; */

    internal SurfaceInfoBuilder(NamespacedKey<DawnSurfaceInfo> key, FootstepSurface value) : base(key, value)
    {
    }

    /* public SurfaceInfoBuilder SetSurfaceTag(string surfaceTag)
    {
        _surfaceTag = surfaceTag;
        return this;
    }

    public SurfaceInfoBuilder SetSurfaceClips(params AudioClip[] surfaceClips)
    {
        _surfaceClips = [.. surfaceClips];
        return this;
    }

    public SurfaceInfoBuilder SetHitSurfaceSFX(AudioClip hitSurfaceSFX)
    {
        _hitSurfaceSFX = hitSurfaceSFX;
        return this;
    } */

    override internal DawnSurfaceInfo Build()
    {
        return new DawnSurfaceInfo(key, [], value, -1, customData);
    }
}