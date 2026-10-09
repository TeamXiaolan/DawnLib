using UnityEngine;

namespace Dawn;

public class DawnFarClipOverride : MonoBehaviour
{
    [field: SerializeField]
    [field: Range(1f, 1000f)]
    public float NewRange { get; private set; } = 400f;

    public void OnEnable()
    {
        Camera camera = GameNetworkManager.Instance.localPlayerController.gameplayCamera;
        if (camera.farClipPlane == 400)
        {
            camera.farClipPlane = NewRange;
        }
        else
        {
            if (camera.farClipPlane > NewRange)
            {
                return;
            }

            camera.farClipPlane = NewRange;
        }
    }

    public void OnDisable()
    {
        GameNetworkManager.Instance.localPlayerController.gameplayCamera.farClipPlane = 400;
    }
}