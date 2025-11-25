using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dawn.Utils;
using DunGen.Graph;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using static Dawn.Internal.DawnMoonNetworker;

namespace Dawn.Internal;
public class DawnDungeonNetworker : NetworkSingleton<DawnDungeonNetworker>
{
    // Dungeon loading stuff:
    // 1. Player pulls lever. ServerRPC
    // 2. Host on ServerRPC figures out what interior to load.
    // 3. Host Rpcs to everyone bundle loading of interior
    // 4. Find/start async loading assetbundle
    // 5. wait
    // 6. once all are loaded, unlock game to keep on going

    private Dictionary<PlayerControllerB, BundleState> _playerStates = new();

    private string? _currentBundlePath = null;
    private AssetBundle? _currentBundle = null;
    private DungeonFlow? _currentlyLoadedDungeonFlow = null;

    private NamespacedKey<DawnDungeonInfo> _currentDungeonKey;
    internal bool allPlayersDone { get; private set; }

    internal void HostDecide(DawnDungeonInfo dungeonInfo)
    {
        QueueMoonSceneLoadingServerRpc(dungeonInfo.Key);
    }

    internal void HostRebroadcastQueue()
    {
        QueueMoonSceneLoadingServerRpc(_currentDungeonKey);
    }

    [ServerRpc]
    private void QueueMoonSceneLoadingServerRpc(NamespacedKey dungeonKey)
    {
        QueueMoonSceneLoadingClientRpc(dungeonKey);
    }

    [ClientRpc]
    private void QueueMoonSceneLoadingClientRpc(NamespacedKey dungeonKey)
    {
        DawnDungeonInfo dungeonInfo = LethalContent.Dungeons[dungeonKey.AsTyped<DawnDungeonInfo>()];

        _playerStates = [];
        foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
        {
            if (!player.isPlayerControlled)
                continue;

            _playerStates[player] = BundleState.Queued;
        }

        CheckReadyAndUpdateUI();
        StartCoroutine(DoDungeonBundleLoading(dungeonInfo));
    }

    private static List<GameObject> _objectsToUnregister = new();

    [ServerRpc]
    internal void SyncSpawnSyncedObjectsServerRpc(bool register)
    {
        SyncSpawnSyncedObjectsClientRpc(register);
    }

    [ClientRpc]
    private void SyncSpawnSyncedObjectsClientRpc(bool register)
    {
        DungeonFlow dungeonFlow = RoundManager.Instance.dungeonFlowTypes[RoundManager.Instance.currentDungeonType].dungeonFlow;

        if (dungeonFlow.GetDawnInfo().ShouldSkipIgnoreOverride())
            return;

        if (register)
        {
            SpawnSyncedObject[] allSpawnSyncedObjects = GameObject.FindObjectsOfType<SpawnSyncedObject>();
            List<GameObject> vanillaSpawnSyncedObjects = new();
            foreach (DawnDungeonInfo dungeonInfo in LethalContent.Dungeons.Values)
            {
                if (!dungeonInfo.ShouldSkipIgnoreOverride())
                    continue;

                foreach (GameObject spawnSyncedObject in dungeonInfo.SpawnSyncedObjects.Select(x => x.spawnPrefab))
                {
                    if (spawnSyncedObject == null)
                        continue;

                    vanillaSpawnSyncedObjects.Add(spawnSyncedObject);
                }
            }

            foreach (SpawnSyncedObject spawnSyncedObject in allSpawnSyncedObjects)
            {
                if (spawnSyncedObject.spawnPrefab == null)
                    continue;

                foreach (GameObject vanillaSpawnSyncedObject in vanillaSpawnSyncedObjects)
                {
                    if (spawnSyncedObject.spawnPrefab.name == vanillaSpawnSyncedObject.name)
                    {
                        Debuggers.Dungeons?.Log($"Fixed SpawnSyncedObject: {spawnSyncedObject.spawnPrefab.name} with vanilla reference");
                        spawnSyncedObject.spawnPrefab = vanillaSpawnSyncedObject;
                        break;
                    }
                }
            }

            foreach (SpawnSyncedObject spawnSyncedObject in allSpawnSyncedObjects)
            {
                if (spawnSyncedObject.spawnPrefab == null || vanillaSpawnSyncedObjects.Contains(spawnSyncedObject.spawnPrefab))
                    continue;

                // TODO: is this even necessary?
                /*if (spawnSyncedObject.spawnPrefab.GetComponent<NetworkObject>() == null)
                {
                    byte[] hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(Key.ToString() + spawnSyncedObject.spawnPrefab.name));
                    NetworkObject networkObject = spawnSyncedObject.spawnPrefab.AddComponent<NetworkObject>();
                    networkObject.GlobalObjectIdHash = BitConverter.ToUInt32(hash, 0);
                }*/

                if (NetworkManager.Singleton.NetworkConfig.Prefabs.Contains(spawnSyncedObject.spawnPrefab))
                    continue;

                _objectsToUnregister.Add(spawnSyncedObject.spawnPrefab);
                NetworkManager.Singleton.AddNetworkPrefab(spawnSyncedObject.spawnPrefab);
            }
        }
        else
        {
            foreach (GameObject obj in _objectsToUnregister)
            {
                NetworkManager.Singleton.RemoveNetworkPrefab(obj);
            }
            _objectsToUnregister.Clear();
        }
    }

    // todo: this is technically insecure. i dont care
    [ServerRpc(RequireOwnership = false)]
    private void PlayerSetBundleStateServerRpc(PlayerControllerReference reference, BundleState state)
    {
        PlayerSetBundleStateClientRpc(reference, state);
    }

    [ClientRpc]
    public void PlayerSetBundleStateClientRpc(PlayerControllerReference reference, BundleState state)
    {
        PlayerControllerB player = reference;

        _playerStates[reference] = state;
        CheckReadyAndUpdateUI();

        if (state == BundleState.Error)
        {
            DawnPlugin.Logger.LogError($"player: {player.playerUsername} failed to load asset bundle!");
        }
        Debuggers.Dungeons?.Log($"Player '{player.playerUsername}' updated their bundle loading state to: {state}.");
    }

    private IEnumerator DoDungeonBundleLoading(DawnDungeonInfo dungeonInfo)
    {
        yield return new WaitForSeconds(0.05f); // here to avoid a race condition, i think a client coudl recieve PlayerReadyToRoute before QueueMoonScene and fuck shit up

        if (Equals(dungeonInfo.TypedKey, _currentDungeonKey))
        {
            PlayerSetBundleStateServerRpc(GameNetworkManager.Instance.localPlayerController, BundleState.Done);
            yield break;
        }
        _currentDungeonKey = dungeonInfo.TypedKey;


        if (!dungeonInfo.ShouldSkipIgnoreOverride())
        {
            if (_currentBundlePath != dungeonInfo.AssetBundlePath)
            {
                if (_currentBundle != null)
                {
                    yield return StartCoroutine(UnloadExisting());
                }

                PlayerSetBundleStateServerRpc(GameNetworkManager.Instance.localPlayerController, BundleState.Loading);
                yield return null;


                AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(dungeonInfo.AssetBundlePath);
                yield return request;

                DungeonFlow? flowToLoad = CheckDungeonBundleFailed(dungeonInfo, request);
                
                // todo: more graceful error handling?
                if (flowToLoad == null)
                {
                    PlayerSetBundleStateServerRpc(GameNetworkManager.Instance.localPlayerController, BundleState.Error);
                    yield break;
                }
                else
                {
                    _currentBundlePath = dungeonInfo.AssetBundlePath;
                    _currentBundle = request.assetBundle;
                    _currentlyLoadedDungeonFlow = flowToLoad;
                }
            }
        }
        else if (_currentBundle != null)
        {
            yield return StartCoroutine(UnloadExisting());
        }

        PlayerSetBundleStateServerRpc(GameNetworkManager.Instance.localPlayerController, BundleState.Done);
    }

    DungeonFlow? CheckDungeonBundleFailed(DawnDungeonInfo dungeonInfo, AssetBundleCreateRequest request)
    {
        if (!request.isDone || request.assetBundle == null)
        {
            return null;
        }

        AssetBundle bundle = request.assetBundle;
        DungeonFlow? loadedFlow = bundle.LoadAsset<DungeonFlow>($"{dungeonInfo.DungeonFlow.name}");
        if (loadedFlow == null)
        {
            DawnPlugin.Logger.LogError($"Bundle: {Path.GetFileName(dungeonInfo.AssetBundlePath)} does not contain DungeonFlow: {dungeonInfo.DungeonFlow.name}.");
            return null;
        }
        return loadedFlow;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        // request that the bundle gets unloaded just before this object gets destroyed. e.g. going back to main menu
        _currentBundle?.Unload(true);
        _currentlyLoadedDungeonFlow = null;
    }

    private IEnumerator UnloadExisting()
    {
        if (_currentBundle == null)
            yield break;

        PlayerSetBundleStateServerRpc(GameNetworkManager.Instance.localPlayerController, BundleState.Unloading);

        yield return _currentBundle.UnloadAsync(true);

        _currentBundle = null;
        _currentlyLoadedDungeonFlow = null;
        _currentBundlePath = null;
    }

    private void CheckReadyAndUpdateUI()
    {
        bool anyFailedPlayers = _playerStates.Any(it => it.Value == BundleState.Error);
        int remainingPlayers = _playerStates.Count(it => it.Value != BundleState.Done);

        Debuggers.Dungeons?.Log($"Dungeon {nameof(CheckReadyAndUpdateUI)}. failed: {anyFailedPlayers}, remaining: {remainingPlayers}");
        Debuggers.Dungeons?.Log($"connected players amount: {_playerStates.Count}. done players = {_playerStates.Count(it => it.Value == BundleState.Done)}");

        if (remainingPlayers <= 0)
        {
            // ready!
            StartCoroutine(UnlockGame());
        }
        else
        {
            LockGame();

            if (anyFailedPlayers)
            {
                // StartMatchLeverRefs.Instance.triggerScript.disabledHoverTip = " [ Someone failed to pre-load the moon, report this! ] ";
            }
            else
            {
                // todo: add a call out for players with bad connection that they're taking their sweet time.
                // StartMatchLeverRefs.Instance.triggerScript.disabledHoverTip = $" [ {remainingPlayers} player(s) still need to load ] ";
            }
        }
    }

    private void LockGame()
    {
        allPlayersDone = false;
    }

    private IEnumerator UnlockGame()
    {
        allPlayersDone = true;
        yield return new WaitUntil(() => allPlayersDone);

        if (_currentlyLoadedDungeonFlow != null && RoundManager.Instance.dungeonFlowTypes[RoundManager.Instance.currentDungeonType].dungeonFlow.name == _currentlyLoadedDungeonFlow.name)
        {
            RoundManager.Instance.dungeonFlowTypes[RoundManager.Instance.currentDungeonType].dungeonFlow = _currentlyLoadedDungeonFlow;
        }
    }
}