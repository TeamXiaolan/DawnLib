using System.Collections.Generic;
using UnityEngine;

namespace Dawn;

public sealed class DawnSurfaceInfo : DawnBaseInfo<DawnSurfaceInfo>
{
    internal DawnSurfaceInfo(NamespacedKey<DawnSurfaceInfo> key, HashSet<NamespacedKey> tags, FootstepSurface surface, List<AudioClip> crouchClips, float volume, GameObject? vainShroudPrefab, bool isNatural, bool quicksandCompatible, GameObject? surfaceVFXPrefab, Vector3 surfaceVFXOffset, int surfaceIndex, IDataContainer? customData) : base(key, tags, customData)
    {
        Surface = surface;
        CrouchClips = crouchClips;
        Volume = volume;

        VainShroudPrefab = vainShroudPrefab;
        if (VainShroudPrefab != null)
        {
            VainShroudMesh = VainShroudPrefab.GetComponent<MeshFilter>().sharedMesh;
            VainShroudMaterials = VainShroudPrefab.GetComponent<MeshRenderer>().sharedMaterials;
        }

        IsNatural = isNatural;
        QuicksandCompatible = quicksandCompatible;

        SurfaceVFXPrefab = surfaceVFXPrefab;
        SurfaceVFXOffset = surfaceVFXOffset;

        SurfaceIndex = surfaceIndex;
    }

    public FootstepSurface Surface { get; internal set; }
    public List<AudioClip> CrouchClips { get; }
    public float Volume { get; }

    public GameObject? VainShroudPrefab { get; }
    public Mesh VainShroudMesh { get; }
    public Material[] VainShroudMaterials { get; }

    public bool IsNatural { get; }
    public bool QuicksandCompatible { get; }

    public GameObject? SurfaceVFXPrefab { get; }
    public Vector3 SurfaceVFXOffset { get; }

    public int SurfaceIndex { get; internal set; } = -1;
}