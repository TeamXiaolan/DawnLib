using GameNetcodeStuff;
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

    /*public void Update() // todo
    {
        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
        if (GravityCenter == null || !localPlayer.TryGetCurrentDawnSurface(out DawnSurface? currentDawnSurface) || currentDawnSurface != this)
        {
            return;
        }

        Transform localPlayerMesh = GameNetworkManager.Instance.localPlayerController.meshContainer;
        localPlayerMesh.transform.up = (GameNetworkManager.Instance.localPlayerController.meshContainer.position - GravityCenter.transform.position).normalized;
    }*/
}