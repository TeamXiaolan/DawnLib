using System.Collections.Generic;
using CodeRebirthLib.ContentManagement.Achievements;
using CodeRebirthLib.ContentManagement.Enemies;
using CodeRebirthLib.Util;
using Unity.Netcode;
using UnityEngine;

namespace CodeRebirthLib.Patches;

static class GameNetworkManagerPatch
{
    private static List<GameObject> _networkPrefabs = new();
    private static bool _alreadyRegisteredNetworkPrefabs = false;

    public static void RegisterCRLibNetworkPrefab(GameObject? prefab)
    {
        if (prefab == null)
        {
            // TODO: throw?
            return;
        }

        if (_alreadyRegisteredNetworkPrefabs)
        {
            // TODO: throw or error/warning?
            return;
        }
        _networkPrefabs.Add(prefab);
    }

    internal static void Init()
    {
        On.GameNetworkManager.Start += GameNetworkManagerOnStart;
        On.GameNetworkManager.SaveLocalPlayerValues += GameNetworkManager_SaveLocalPlayerValues;
        On.GameNetworkManager.SaveItemsInShip += GameNetworkManager_SaveItemsInShip;
        On.GameNetworkManager.ResetSavedGameValues += GameNetworkManager_ResetSavedGameValues;
    }

    private static void GameNetworkManager_SaveLocalPlayerValues(On.GameNetworkManager.orig_SaveLocalPlayerValues orig, GameNetworkManager self)
    {
        orig(self);
        CRAchievementHandler.SaveAll();
    }

    private static void GameNetworkManagerOnStart(On.GameNetworkManager.orig_Start orig, GameNetworkManager self)
    {
        orig(self);
        foreach (GameObject networkPrefab in _networkPrefabs)
        {
            if (NetworkManager.Singleton.NetworkConfig.Prefabs.Contains(networkPrefab))
                continue;

            NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);
        }
        VanillaEnemies.Init();
        _alreadyRegisteredNetworkPrefabs = true;
    }

    private static void GameNetworkManager_SaveItemsInShip(On.GameNetworkManager.orig_SaveItemsInShip orig, GameNetworkManager self)
    {
        orig(self);
        CodeRebirthLibPlugin.ExtendedLogging("Saving CodeRebirthLibData");
        CodeRebirthLibNetworker.Instance?.SaveCodeRebirthLibData();
    }

    private static void GameNetworkManager_ResetSavedGameValues(On.GameNetworkManager.orig_ResetSavedGameValues orig, GameNetworkManager self)
    {
        orig(self);
        ES3Settings settings;
        if (CodeRebirthLibNetworker.Instance != null)
        {
            settings = CodeRebirthLibNetworker.Instance.SaveSettings;
        }
        else
        {
            settings = new ES3Settings($"CRLib{GameNetworkManager.Instance.currentSaveFileName}", ES3.EncryptionType.None);
        }
        CodeRebirthLibNetworker.ResetCodeRebirthLibData(settings);
    }
}