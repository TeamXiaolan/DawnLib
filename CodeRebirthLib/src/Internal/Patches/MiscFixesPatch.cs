using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace CodeRebirthLib;

static class MiscFixesPatch
{
    internal static List<GameObject> networkPrefabsToAdd = new();
    internal static List<GameObject> soundPrefabsToFix = new();

    internal static void Init()
    {
        On.GameNetworkManager.Start += AddNetworkPrefabToNetworkConfig;

    }

    private static void AddNetworkPrefabToNetworkConfig(On.GameNetworkManager.orig_Start orig, GameNetworkManager self)
    {
        orig(self);
        foreach (GameObject networkPrefab in networkPrefabsToAdd)
        {
            if (NetworkManager.Singleton.NetworkConfig.Prefabs.Contains(networkPrefab))
                continue;

            NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);
        }
    }
}