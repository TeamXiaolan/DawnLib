using System;
using Dawn.Internal;
using Unity.Netcode;
using UnityEngine;

namespace Dawn.Utils;

[RequireComponent(typeof(Collider))]
[AddComponentMenu($"{DawnConstants.MenuName}/Dawn Surface")]
public class DawnSurface : MonoBehaviour
{
    [field: SerializeField]
    [field: InspectorName("Namespace")]
    public NamespacedKey NamespacedKey { get; private set; }
    public int SurfaceIndex { get; private set; } = -1;

    /* public void Start()
    {
        if (!LethalContent.Surfaces.TryGetValue(NamespacedKey, out DawnSurfaceInfo surfaceInfo))
        {
            DawnPlugin.Logger.LogWarning($"Surface: '{NamespacedKey}' not found.");
            return;
        }

        if (surfaceInfo.Surface == null)
        {
            DawnPlugin.Logger.LogWarning($"Surface: '{NamespacedKey}' has no footstep surface defined.");
            return;
        }
    } */
}