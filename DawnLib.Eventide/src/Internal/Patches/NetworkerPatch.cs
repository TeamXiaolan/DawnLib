using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eventide.Internal;

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
            if (!EventideNetworker.Instance)
            {
                GameObject networkerInstance = Object.Instantiate(EventidePlugin.EventideMainNetwork.NetworkerPrefab);
                SceneManager.MoveGameObjectToScene(networkerInstance, self.gameObject.scene);
                networkerInstance.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}