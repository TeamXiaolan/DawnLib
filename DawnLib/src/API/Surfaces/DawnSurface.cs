using UnityEngine;

namespace Dawn.Utils;

[RequireComponent(typeof(Collider))]
[AddComponentMenu($"{DawnConstants.MenuName}/Dawn Surface")]
public class DawnSurface : MonoBehaviour
{
    [field: SerializeField]
    [field: InspectorName("Namespace")]
    public NamespacedKey NamespacedKey { get; private set; }

    [field: SerializeField]
    [field: InspectorName("Center Of Gravity")]
    public GameObject? GravityCenter { get; private set; }

    [field: SerializeField]
    public float GravityStrength { get; private set; } = 1f;

    public int SurfaceIndex { get; private set; } = -1;

    public void Start()
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

        SurfaceIndex = surfaceInfo.SurfaceIndex;
    }
}