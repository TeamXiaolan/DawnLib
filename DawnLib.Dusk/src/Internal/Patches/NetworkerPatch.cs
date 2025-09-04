using Dawn;
using Dawn.Internal;
using Dawn.Utils;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dusk.Internal;

static class NetworkerPatch
{
    internal static void Init()
    {
        On.StartOfRound.Start += CreateNetworker;
    }

    private static void CreateNetworker(On.StartOfRound.orig_Start orig, StartOfRound self)
    {
        orig(self);
        MoreLayerMasks.Init();

        self.NetworkObject.OnSpawn(() =>
        {
            if (self.IsServer || self.IsHost)
            {
                if (!DawnNetworker.Instance)
                {
                    GameObject networkerInstance = Object.Instantiate(DuskPlugin.Main.NetworkerPrefab);
                    SceneManager.MoveGameObjectToScene(networkerInstance, self.gameObject.scene);
                    networkerInstance.GetComponent<NetworkObject>().Spawn();
                }
            }

            if (AchievementUIGetCanvas.Instance == null) Object.Instantiate(DuskPlugin.Main.AchievementGetUICanvasPrefab);
        });
    }
}