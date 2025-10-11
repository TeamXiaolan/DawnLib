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

    private Dictionary<PlayerControllerB, BundleState> _playerStates = new();

    private string? _currentBundlePath = null;
    private AssetBundle? _currentBundle = null;

    private NamespacedKey<DawnMoonInfo> _currentMoonKey;
    private NamespacedKey<IMoonSceneInfo> _currentSceneKey;

    internal bool allPlayersDone { get; private set; }

    public enum BundleState
    {
        Queued,
        Unloading,
        Loading,
        Error,
        Done
    }

    private AnimatorOverrideController _animatorOverrideController;
    private RuntimeAnimatorController _originalAnimatorController;
    private AnimationClip _originalShipLandClip, _originalShipLeaveClip;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _originalAnimatorController = StartOfRoundRefs.Instance.shipAnimator.runtimeAnimatorController;
        foreach (AnimationClip animationClip in StartOfRoundRefs.Instance.shipAnimator.runtimeAnimatorController.animationClips)
        {
            if (animationClip.name == "HangarShipLandB")
            {
                _originalShipLandClip = animationClip;
                continue;
            }

            if (animationClip.name == "ShipLeave")
            {
                _originalShipLeaveClip = animationClip;
                continue;
            }
        }
    }

    internal void HostDecide(DawnMoonInfo moonInfo)
    {
        int totalWeight = moonInfo.Scenes.Sum(it => it.Weight.Provide());

        System.Random sceneRandom = new(StartOfRoundRefs.Instance.randomMapSeed + 502 + 0);
        int chosenWeight = sceneRandom.Next(0, totalWeight);

        IMoonSceneInfo sceneInfo = moonInfo.Scenes[0];
        for (int i = 0; i < moonInfo.Scenes.Count; i++)
        {
            sceneInfo = moonInfo.Scenes[i];
            chosenWeight -= sceneInfo.Weight.Provide();
            if (chosenWeight <= 0)
            {
                break;
            }
        }

        moonInfo.Level.sceneName = sceneInfo.SceneName;
        QueueMoonSceneLoadingServerRpc(moonInfo.Key, sceneInfo.Key);
    }

    internal void HostRebroadcastQueue()
    {
        QueueMoonSceneLoadingServerRpc(_currentMoonKey, _currentSceneKey);
    }

    [ServerRpc()]
    private void QueueMoonSceneLoadingServerRpc(NamespacedKey moonKey, NamespacedKey sceneKey)
    {
        QueueMoonSceneLoadingClientRpc(moonKey, sceneKey);
    }

    [ClientRpc]
    private void QueueMoonSceneLoadingClientRpc(NamespacedKey moonKey, NamespacedKey sceneKey)
    {
        DawnMoonInfo moonInfo = LethalContent.Moons[moonKey.AsTyped<DawnMoonInfo>()];
        IMoonSceneInfo sceneInfo = moonInfo.Scenes.First(it => Equals(it.Key, sceneKey));

        _playerStates = [];
        foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
        {
            if (!player.isPlayerControlled)
                continue;

            _playerStates[player] = BundleState.Queued;
        }

        CheckReadyAndUpdateUI();
        StartCoroutine(DoMoonSceneLoading(moonInfo, sceneInfo));
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
        Debuggers.Moons?.Log($"Player '{player.playerUsername}' updated their bundle loading state to: {state}.");
    }

    private IEnumerator DoMoonSceneLoading(DawnMoonInfo moonInfo, IMoonSceneInfo sceneInfo)
    {
        yield return new WaitForSeconds(0.05f); // here to avoid a race condition, i think a client coudl recieve PlayerReadyToRoute before QueueMoonScene and fuck shit up

        if (Equals(moonInfo.TypedKey, _currentMoonKey) && Equals(sceneInfo.TypedKey, _currentSceneKey))
        {
            PlayerSetBundleStateServerRpc(GameNetworkManager.Instance.localPlayerController, BundleState.Done);
            yield break;
        }
        _currentMoonKey = moonInfo.TypedKey;
        _currentSceneKey = sceneInfo.TypedKey;

        StartOfRound.Instance.shipAnimator.runtimeAnimatorController = _originalAnimatorController;
        _animatorOverrideController = new AnimatorOverrideController(StartOfRound.Instance.shipAnimator.runtimeAnimatorController);
        StartOfRound.Instance.shipAnimator.runtimeAnimatorController = _animatorOverrideController;

        if (sceneInfo is CustomMoonSceneInfo customMoon)
        {
            ReplaceShipAnimations(_originalShipLeaveClip, customMoon.ShipLandingOverrideAnimation);
            ReplaceShipAnimations(_originalShipLandClip, customMoon.ShipLandingOverrideAnimation);
            if (_currentBundlePath != customMoon.AssetBundlePath)
            {
                if (_currentBundle != null)
                {
                    yield return StartCoroutine(UnloadExisting());
                }

                PlayerSetBundleStateServerRpc(GameNetworkManager.Instance.localPlayerController, BundleState.Loading);
                yield return null;

                AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(customMoon.AssetBundlePath);
                yield return request;

                // todo: more graceful error handling?
                if (!request.isDone || request.assetBundle == null)
                {
                    PlayerSetBundleStateServerRpc(GameNetworkManager.Instance.localPlayerController, BundleState.Error);
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

        PlayerSetBundleStateServerRpc(GameNetworkManager.Instance.localPlayerController, BundleState.Done);
    }

    public void ReplaceShipAnimations(AnimationClip originalClip, AnimationClip? newClip)
    {
        _animatorOverrideController[originalClip] = newClip;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        // request that the bundle gets unloaded just before this object gets destroyed. e.g. going back to main menu
        _currentBundle?.Unload(true);
    }

    private IEnumerator UnloadExisting()
    {
        if (_currentBundle == null)
            yield break;

        PlayerSetBundleStateServerRpc(GameNetworkManager.Instance.localPlayerController, BundleState.Unloading);

        yield return _currentBundle.UnloadAsync(true);

        _currentBundle = null;
        _currentBundlePath = null;
    }

    private void CheckReadyAndUpdateUI()
    {
        bool anyFailedPlayers = _playerStates.Any(it => it.Value == BundleState.Error);
        int remainingPlayers = _playerStates.Count(it => it.Value != BundleState.Done);

        Debuggers.Moons?.Log($"{nameof(CheckReadyAndUpdateUI)}. failed: {anyFailedPlayers}, remaining: {remainingPlayers}");
        Debuggers.Moons?.Log($"connected players amount: {_playerStates.Count}. done players = {_playerStates.Count(it => it.Value == BundleState.Done)}");

        if (remainingPlayers <= 0)
        {
            // ready!
            StartCoroutine(UnlockLever());
        }
        else
        {
            LockLever();

            if (anyFailedPlayers)
            {
                StartMatchLeverRefs.Instance.triggerScript.disabledHoverTip = " [ Someone failed to pre-load the moon, report this! ] ";
            }
            else
            {
                // todo: add a call out for players with bad connection that they're taking their sweet time.
                StartMatchLeverRefs.Instance.triggerScript.disabledHoverTip = $" [ {remainingPlayers} player(s) still need to load ] ";
            }
        }
    }

    private void LockLever()
    {
        StartMatchLeverRefs.Instance.triggerScript.interactable = false;
        allPlayersDone = false;
        StartOfRound.Instance.travellingToNewLevel = true;
    }

    private IEnumerator UnlockLever()
    {
        allPlayersDone = true;
        yield return new WaitUntil(() => StartOfRound.Instance.shipTravelCoroutine == null);
        StartMatchLeverRefs.Instance.triggerScript.interactable = true;
        StartOfRound.Instance.travellingToNewLevel = false;
    }
}