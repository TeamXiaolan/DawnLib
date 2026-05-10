using System.Collections.Generic;
using UnityEngine;

namespace Dawn;

public class SurfaceInfoBuilder : BaseInfoBuilder<DawnSurfaceInfo, FootstepSurface, SurfaceInfoBuilder>
{
    private GameObject? _surfaceVFXPrefab = null;
    private Vector3 _surfaceVFXOffset = Vector3.zero;
    private bool _isNatural, _quicksandCompatible = false;
    private List<AudioClip> _crouchClips = new List<AudioClip>();
    private float _volume = 1f;

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
        _quicksandCompatible = quicksandCompatible;
        return this;
    }

    public SurfaceInfoBuilder SetCrouchClips(List<AudioClip> crouchClips)
    {
        _crouchClips = crouchClips;
        return this;
    }

    public SurfaceInfoBuilder OverrideVolume(float volume)
    {
        _volume = volume;
        return this;
    }

    override internal DawnSurfaceInfo Build()
    {
        value.surfaceTag = "AnomalyObject";
        return new DawnSurfaceInfo(key, [], value, _crouchClips, _volume, _isNatural, _quicksandCompatible, _surfaceVFXPrefab, _surfaceVFXOffset, -1, customData);
    }
}