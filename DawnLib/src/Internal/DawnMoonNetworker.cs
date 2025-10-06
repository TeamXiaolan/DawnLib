using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dawn.Utils;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace Dawn.Internal;
public class DawnMoonNetworker : NetworkSingleton<DawnMoonNetworker>
{
    // Moon loading stuff:
    // 1. Player uses terminal to route to a new moon. ServerRPC
    // 2. Host on ServerRPC chooses random scene
    // 3. Host Rpcs to everyone moon and scene
    // 4. Find/start loading assetbundle, update screen ui
    // 5. wait
    // 6. once all are loaded, unlock start match lever

    private StartMatchLever _lever;
    private Dictionary<PlayerControllerB, BundleState> _playerStates;

    private string? _currentBundlePath = null;
    private AssetBundle? _currentBundle;

    private NamespacedKey<DawnMoonInfo> _currentMoonKey;
    private NamespacedKey<MoonSceneInfo> _currentSceneKey;
    
    internal bool allPlayersDone { get; private set; }
    
    public enum BundleState
    {
        Queued,
        Unloading,
        Loading,
        Error,
        Done
    }
    
    private void Start()
    {
        _lever = FindFirstObjectByType<StartMatchLever>();
    }

    internal void HostDecide(DawnMoonInfo moonInfo)
    {
        int totalWeight = moonInfo.Scenes.Sum(it => it.Weight.Provide());
        
        // this should probably be changed to a system random? and should be selected probably similar to how weather gets selected where its still determinstic.
        int chosenWeight = UnityEngine.Random.Range(0, totalWeight);
        
        MoonSceneInfo sceneInfo = moonInfo.Scenes[0];
        for(int i = 0; i < moonInfo.Scenes.Count; i++)
        {
            sceneInfo = moonInfo.Scenes[i];
            chosenWeight -= sceneInfo.Weight.Provide();
            if(chosenWeight <= 0) break;
        }

        moonInfo.Level.sceneName = sceneInfo.SceneName;
        QueueMoonSceneLoadingRPC(moonInfo.Key, sceneInfo.Key);
    }

    internal void HostRebroadcastQueue()
    {
        QueueMoonSceneLoadingRPC(_currentMoonKey, _currentSceneKey);
    }
    
    [Rpc(SendTo.Everyone)]
    void QueueMoonSceneLoadingRPC(NamespacedKey moonKey, NamespacedKey sceneKey)
    {
        
        DawnMoonInfo moonInfo = LethalContent.Moons[moonKey.AsTyped<DawnMoonInfo>()];
        MoonSceneInfo sceneInfo = moonInfo.Scenes.First(it => Equals(it.Key, sceneKey));

        _playerStates = [];
        foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
        {
            if(!player.isPlayerControlled) continue;

            _playerStates[player] = BundleState.Queued;
        }
        
        CheckReadyAndUpdateUI();
        StartCoroutine(DoMoonSceneLoading(moonInfo, sceneInfo));
    }

    // todo: this is technically insecure. i dont care
    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    void PlayerSetBundleStateRPC(PlayerControllerReference reference, BundleState state)
    {
        PlayerControllerB player = reference;
        
        _playerStates[reference] = state;
        CheckReadyAndUpdateUI();

        if (state == BundleState.Error)
        {
            DawnPlugin.Logger.LogError($"player: {player.playerUsername} failed to load asset bundle!");
        }
        Debuggers.Moons?.Log($"Player '{player.playerUsername}' updated their bundle loading state to: {state}.");
    }

    IEnumerator DoMoonSceneLoading(DawnMoonInfo moonInfo, MoonSceneInfo sceneInfo)
    {
        yield return new WaitForSeconds(.05f); // here to avoid a race condition, i think a client coudl recieve PlayerReadyToRoute before QueueMoonScene and fuck shit up
        
        
        if (Equals(moonInfo.TypedKey, _currentMoonKey) && Equals(sceneInfo.TypedKey, _currentSceneKey))
        {
            PlayerSetBundleStateRPC(GameNetworkManager.Instance.localPlayerController, BundleState.Done);
            yield break;
        }
        _currentMoonKey = moonInfo.TypedKey;
        _currentSceneKey = sceneInfo.TypedKey;
        
        if (sceneInfo is CustomMoonSceneInfo customMoon)
        {
            if (_currentBundlePath != customMoon.AssetBundlePath)
            {
                if (_currentBundle != null)
                {
                    yield return StartCoroutine(UnloadExisting());
                }
                
                PlayerSetBundleStateRPC(GameNetworkManager.Instance.localPlayerController, BundleState.Loading);
                yield return null;
                
                AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(customMoon.AssetBundlePath);
                yield return request;
                
                // todo: more graceful error handling?
                if (!request.isDone || request.assetBundle == null)
                {
                    PlayerSetBundleStateRPC(GameNetworkManager.Instance.localPlayerController, BundleState.Error);
                    yield break;
                }
                else
                {
                    _currentBundlePath = customMoon.AssetBundlePath;
                    _currentBundle = request.assetBundle;
                }
            }
        } 
        else if (_currentBundle != null)
        {
            yield return StartCoroutine(UnloadExisting());
        }
        
        PlayerSetBundleStateRPC(GameNetworkManager.Instance.localPlayerController, BundleState.Done);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
        // request that the bundle gets unloaded just before this object gets destroyed. e.g. going back to main menu
        if(_currentBundle != null)
            _currentBundle.Unload(true);
    }

    IEnumerator UnloadExisting()
    {
        if(_currentBundle == null)
            yield break;
        
        PlayerSetBundleStateRPC(GameNetworkManager.Instance.localPlayerController, BundleState.Unloading);

        yield return _currentBundle.UnloadAsync(true);
                    
        _currentBundle = null;
        _currentBundlePath = null;
    }

    void CheckReadyAndUpdateUI()
    {
        bool anyFailedPlayers = _playerStates.Any(it => it.Value == BundleState.Error);
        int remainingPlayers = _playerStates.Count(it => it.Value != BundleState.Done);
        
        Debuggers.Moons?.Log($"{nameof(CheckReadyAndUpdateUI)}. failed: {anyFailedPlayers}, remaining: {remainingPlayers}");
        Debuggers.Moons?.Log($"connected players amount: {_playerStates.Count}. done players = {_playerStates.Count(it => it.Value == BundleState.Done)}");

        if (remainingPlayers <= 0)
        {
            // ready!
            UnlockLever();
        }
        else
        {
            LockLever();

            if (anyFailedPlayers)
            {
                _lever.triggerScript.disabledHoverTip = " [ Someone failed to pre-load the moon, report this! ] ";
            }
            else
            {
                _lever.triggerScript.disabledHoverTip = $" [ {remainingPlayers} player(s) still need to load ] ";
            }
        }
    }

    void LockLever()
    {
        _lever.triggerScript.interactable = false;
        allPlayersDone = false;
        StartOfRound.Instance.travellingToNewLevel = true;
    }

    void UnlockLever()
    {
        allPlayersDone = true;
        _lever.triggerScript.interactable = true;
        StartOfRound.Instance.travellingToNewLevel = false;
    }
}