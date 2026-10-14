using Dawn.Internal;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nightfall.Internal;

static class NetworkerPatch
{
    internal static void Init()
    {
        On.StartOfRound.Awake += CreateNetworker;
    }

    private static void CreateNetworker(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        orig(self);
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
        {
            if (!NightfallNetworker.Instance)
            {
                GameObject networkerInstance = Object.Instantiate(NightfallPlugin.NightfallMainNetwork.NetworkerPrefab);
                SceneManager.MoveGameObjectToScene(networkerInstance, self.gameObject.scene);
                networkerInstance.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}