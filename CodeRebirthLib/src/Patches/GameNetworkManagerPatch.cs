using System;
using System.Text;
using CodeRebirthLib.ContentManagement.Enemies;
using CodeRebirthLib.Util;
using Unity.Netcode;
using UnityEngine;

namespace CodeRebirthLib.Patches;
static class GameNetworkManagerPatch
{
    internal static void Init()
    {
        On.GameNetworkManager.Start += GameNetworkManagerOnStart;
    }
    private static void GameNetworkManagerOnStart(On.GameNetworkManager.orig_Start orig, GameNetworkManager self)
    {
        GameObject prefab = new GameObject(nameof(CodeRebirthLibNetworker));
        prefab.hideFlags = HideFlags.HideAndDontSave;

        NetworkObject networkObject = prefab.AddComponent<NetworkObject>();
        networkObject.GlobalObjectIdHash = BitConverter.ToUInt32(Encoding.UTF8.GetBytes(prefab.name), 0);

        prefab.AddComponent<CodeRebirthLibNetworker>();
        NetworkManager.Singleton.AddNetworkPrefab(prefab);
        CodeRebirthLibNetworker.prefab = prefab;
        
        orig(self);
        VanillaEnemies.Init();
    }
}