using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dawn.Utils;
using DunGen;
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
    private DungeonFlow? _temporaryDungeonFlow = null;

    private NamespacedKey<DawnDungeonInfo> _currentDungeonKey;
    internal bool allPlayersDone { get; private set; }

    internal void HostDecide(DawnDungeonInfo dungeonInfo)
    {
        QueueDungeonBundleLoadingServerRpc(dungeonInfo.Key);
    }

    internal void HostRebroadcastQueue()
    {
        if (_currentDungeonKey == default) return;
        QueueDungeonBundleLoadingServerRpc(_currentDungeonKey);
    }

    [ServerRpc]
    private void QueueDungeonBundleLoadingServerRpc(NamespacedKey dungeonKey)
    {
        QueueDungeonBundleLoadingClientRpc(dungeonKey);
    }

    [ClientRpc]
    private void QueueDungeonBundleLoadingClientRpc(NamespacedKey dungeonKey)
    {
        QueueDungeonBundleLoading(dungeonKey);
    }

    internal void QueueDungeonBundleLoading(NamespacedKey dungeonKey)
    {
        DawnDungeonInfo dungeonInfo = LethalContent.Dungeons[dungeonKey.AsTyped<DawnDungeonInfo>()];

        _playerStates = [];
        foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
        {
            if (!player.isPlayerControlled)
                continue;

            _playerStates[player] = BundleState.Queued;
        }

        CheckReady();
        StartCoroutine(DoDungeonBundleLoading(dungeonInfo));
    }

    private static List<GameObject> _objectsToUnregister = new();

    internal void SyncSpawnSyncedObjects(bool register)
    {
        DawnDungeonInfo importantDungeonInfo = LethalContent.Dungeons[_currentDungeonKey];
        if (importantDungeonInfo.ShouldSkipIgnoreOverride())
            return;

        NetworkManager.Singleton.NetworkConfig.ForceSamePrefabs = false;
        if (register)
        {
            List<GameObject> potentialPrefabs = new();
            foreach (NetworkPrefab networkPrefabs in NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs)
            {
                potentialPrefabs.Add(networkPrefabs.Prefab);
            }

            foreach (SpawnSyncedObject spawnSyncedObject in importantDungeonInfo.SpawnSyncedObjects)
            {
                if (spawnSyncedObject.spawnPrefab == null)
                    continue;

                foreach (GameObject potentialPrefab in potentialPrefabs)
                {
                    if (spawnSyncedObject.spawnPrefab.name == potentialPrefab.name)
                    {
                        Debuggers.Dungeons?.Log($"Fixed SpawnSyncedObject: {spawnSyncedObject.spawnPrefab.name} with reference");
                        spawnSyncedObject.spawnPrefab = potentialPrefab;
                        break;
                    }
                }
            }

            foreach (SpawnSyncedObject spawnSyncedObject in importantDungeonInfo.SpawnSyncedObjects)
            {
                if (spawnSyncedObject.spawnPrefab == null || spawnSyncedObject.spawnPrefab.GetComponent<NetworkObject>() == null)
                    continue;

                if (NetworkManager.Singleton.NetworkConfig.Prefabs.Contains(spawnSyncedObject.spawnPrefab))
                    continue;

                _objectsToUnregister.Add(spawnSyncedObject.spawnPrefab);
                NetworkManager.Singleton.PrefabHandler.AddNetworkPrefab(spawnSyncedObject.spawnPrefab);
                NetworkObject netObj = spawnSyncedObject.spawnPrefab.GetComponent<NetworkObject>();
                DawnPlugin.Logger.LogDebug(
                    $"Registered network prefab '{spawnSyncedObject.spawnPrefab.name}' " +
                    $"with hash={netObj.GlobalObjectIdHash} " +
                    $"(IsServer={NetworkManager.Singleton.IsServer}, IsClient={NetworkManager.Singleton.IsClient})");
            }
        }
        else
        {
            foreach (GameObject obj in _objectsToUnregister)
            {
                NetworkManager.Singleton.PrefabHandler.RemoveNetworkPrefab(obj);
            }
            _objectsToUnregister.Clear();
        }
        NetworkManager.Singleton.NetworkConfig.ForceSamePrefabs = true;
    }

    // todo: this is technically insecure. i dont care
    [ServerRpc(RequireOwnership = false)]
    internal void PlayerSetBundleStateServerRpc(PlayerControllerReference reference, BundleState state)
    {
        PlayerSetBundleStateClientRpc(reference, state);
    }

    [ClientRpc]
    public void PlayerSetBundleStateClientRpc(PlayerControllerReference reference, BundleState state)
    {
        PlayerControllerB player = reference;

        _playerStates[reference] = state;
        CheckReady();

        if (state == BundleState.Error)
        {
            DawnPlugin.Logger.LogError($"player: {player.playerUsername} failed to load asset bundle!");
        }
        Debuggers.Dungeons?.Log($"Player '{player.playerUsername}' updated their bundle loading state to: {state}.");
    }

    private IEnumerator DoDungeonBundleLoading(DawnDungeonInfo dungeonInfo)
    {
        yield return new WaitForSeconds(0.05f); // here to avoid a race condition.

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
        if (_currentBundle == null)
            return;

        SyncSpawnSyncedObjects(false);
        _currentBundle.Unload(true);
    }

    internal IEnumerator UnloadExisting()
    {
        if (_currentBundle == null)
            yield break;

        PlayerSetBundleStateServerRpc(GameNetworkManager.Instance.localPlayerController, BundleState.Unloading);

        SyncSpawnSyncedObjects(false);

        DawnDungeonInfo dungeonInfo = LethalContent.Dungeons[_currentDungeonKey];
        DungeonFlow flowToClear = dungeonInfo.DungeonFlow;
        SwapReferences(ref flowToClear, ref _temporaryDungeonFlow!);

        dungeonInfo.sockets.Clear();
        dungeonInfo.doorways.Clear();
        dungeonInfo.spawnSyncedObjects.Clear();
        dungeonInfo.tiles.Clear();

        _currentlyLoadedDungeonFlow = null;
        ScriptableObject.Destroy(_temporaryDungeonFlow);
        _temporaryDungeonFlow = null;
        _currentDungeonKey = default;

        yield return _currentBundle.UnloadAsync(true);
        _currentBundle = null;
        _currentBundlePath = null;
        yield return Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }

    private void CheckReady()
    {
        bool anyFailedPlayers = _playerStates.Any(it => it.Value == BundleState.Error);
        int remainingPlayers = _playerStates.Count(it => it.Value != BundleState.Done);

        Debuggers.Dungeons?.Log($"Dungeon {nameof(CheckReady)}. failed: {anyFailedPlayers}, remaining: {remainingPlayers}");
        Debuggers.Dungeons?.Log($"connected players amount: {_playerStates.Count}. done players = {_playerStates.Count(it => it.Value == BundleState.Done)}");

        if (remainingPlayers <= 0)
        {
            UnlockGame();
        }
        else
        {
            LockGame();
        }
    }

    private void LockGame()
    {
        allPlayersDone = false;
    }

    private void UnlockGame()
    {
        if (_currentlyLoadedDungeonFlow != null && RoundManager.Instance.dungeonFlowTypes[RoundManager.Instance.currentDungeonType].dungeonFlow.name == _currentlyLoadedDungeonFlow.name)
        {
            DungeonFlow fakeFlow = RoundManager.Instance.dungeonFlowTypes[RoundManager.Instance.currentDungeonType].dungeonFlow;
            _temporaryDungeonFlow = ScriptableObject.CreateInstance<DungeonFlow>();
            SwapReferences(ref _temporaryDungeonFlow, ref fakeFlow);
            SwapReferences(ref fakeFlow, ref _currentlyLoadedDungeonFlow);

            foreach (TileSet tileSet in fakeFlow.GetUsedTileSets())
            {
                Debuggers.Dungeons?.Log($"tileSet.name: {tileSet.name}");
                foreach (DawnTileSetInfo tileSetInfo in LethalContent.TileSets.Values)
                {
                    Debuggers.Dungeons?.Log($"tileSetInfo.Key.Key: {tileSetInfo.Key.Key}");
                    if (tileSetInfo.ShouldSkipIgnoreOverride())
                        continue;

                    if (tileSet.name.Replace(" ", "_").Equals(tileSetInfo.Key.Key, System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        tileSet.SetDawnInfo(tileSetInfo);
                        break;
                    }
                }
            }

            foreach (DungeonArchetype dungeonArchetype in fakeFlow.GetUsedArchetypes())
            {
                Debuggers.Dungeons?.Log($"dungeonArchetype.name: {dungeonArchetype.name}");
                foreach (DawnArchetypeInfo archetypeInfo in LethalContent.Archetypes.Values)
                {
                    if (archetypeInfo.ShouldSkipIgnoreOverride())
                        continue;

                    Debuggers.Dungeons?.Log($"archetypeInfo.Key.Key: {archetypeInfo.Key.Key}");

                    if (dungeonArchetype.name.Replace(" ", "_").Equals(archetypeInfo.Key.Key, System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        dungeonArchetype.SetDawnInfo(archetypeInfo);
                        break;
                    }
                }
            }

            DawnDungeonInfo dungeonInfo = fakeFlow.GetDawnInfo();
            dungeonInfo.sockets = new();
            var tileSets = fakeFlow.TileInjectionRules.Select(it => it.TileSet).Concat(fakeFlow.GetUsedTileSets()).Distinct();
            dungeonInfo.tiles = tileSets.Select(it => it.TileWeights.Weights).SelectMany(it => it).SelectMany(it => it.Value.GetComponentsInChildren<Tile>()).ToList();
            dungeonInfo.doorways = new();
            dungeonInfo.spawnSyncedObjects = new();

            foreach (Tile dungeonTile in dungeonInfo.Tiles)
            {
                foreach (Doorway dungeonDoorway in dungeonTile.gameObject.GetComponentsInChildren<Doorway>())
                {
                    if (!dungeonInfo.Doorways.Contains(dungeonDoorway))
                    {
                        dungeonInfo.doorways.Add(dungeonDoorway);
                    }

                    if (!dungeonInfo.Sockets.Contains(dungeonDoorway.socket))
                    {
                        dungeonInfo.sockets.Add(dungeonDoorway.socket);
                    }

                    foreach (GameObjectWeight doorwayTileWeight in dungeonDoorway.ConnectorPrefabWeights)
                    {
                        foreach (SpawnSyncedObject spawnSyncedObject in doorwayTileWeight.GameObject.GetComponentsInChildren<SpawnSyncedObject>())
                        {
                            if (!dungeonInfo.SpawnSyncedObjects.Contains(spawnSyncedObject))
                            {
                                dungeonInfo.spawnSyncedObjects.Add(spawnSyncedObject);
                            }
                        }
                    }


                    foreach (GameObjectWeight doorwayTileWeight in dungeonDoorway.BlockerPrefabWeights)
                    {
                        foreach (SpawnSyncedObject spawnSyncedObject in doorwayTileWeight.GameObject.GetComponentsInChildren<SpawnSyncedObject>())
                        {
                            if (!dungeonInfo.SpawnSyncedObjects.Contains(spawnSyncedObject))
                            {
                                dungeonInfo.spawnSyncedObjects.Add(spawnSyncedObject);
                            }
                        }
                    }
                }

                foreach (SpawnSyncedObject spawnSyncedObject in dungeonTile.gameObject.GetComponentsInChildren<SpawnSyncedObject>())
                {
                    if (!dungeonInfo.SpawnSyncedObjects.Contains(spawnSyncedObject))
                    {
                        dungeonInfo.spawnSyncedObjects.Add(spawnSyncedObject);
                    }
                }
            }
            SyncSpawnSyncedObjects(true);
        }
        allPlayersDone = true;
    }

    private void SwapReferences(ref DungeonFlow dungeonA, ref DungeonFlow dungeonB)
    {
        dungeonA.globalPropGroupID_obsolete = dungeonB.globalPropGroupID_obsolete;
        dungeonA.globalPropRanges_obsolete = dungeonB.globalPropRanges_obsolete;
        dungeonA.Length = dungeonB.Length;
        dungeonA.BranchMode = dungeonB.BranchMode;
        dungeonA.BranchCount = dungeonB.BranchCount;
        dungeonA.GlobalProps = dungeonB.GlobalProps;
        dungeonA.KeyManager = dungeonB.KeyManager;
        dungeonA.DoorwayConnectionChance = dungeonB.DoorwayConnectionChance;
        dungeonA.RestrictConnectionToSameSection = dungeonB.RestrictConnectionToSameSection;
        dungeonA.TileInjectionRules = dungeonB.TileInjectionRules;
        dungeonA.TileTagConnectionMode = dungeonB.TileTagConnectionMode;
        dungeonA.TileConnectionTags = dungeonB.TileConnectionTags;
        dungeonA.BranchTagPruneMode = dungeonB.BranchTagPruneMode;
        dungeonA.BranchPruneTags = dungeonB.BranchPruneTags;
        dungeonA.Nodes = dungeonB.Nodes;
        dungeonA.Lines = dungeonB.Lines;
        foreach (GraphNode node in dungeonA.Nodes)
        {
            node.Graph = dungeonA;
        }
        foreach (GraphLine line in dungeonA.Lines)
        {
            line.Graph = dungeonA;
        }
        dungeonA.currentFileVersion = dungeonB.currentFileVersion;
    }
}