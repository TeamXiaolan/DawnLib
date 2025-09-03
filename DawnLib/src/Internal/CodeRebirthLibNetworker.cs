using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dawn.Dusk;
using Dawn.Utils;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace Dawn.Internal;

public class CodeRebirthLibNetworker : NetworkSingleton<CodeRebirthLibNetworker>
{
    internal static EntranceTeleport[] _entrancePoints = [];
    internal System.Random CRLibRandom = new();
    internal ES3Settings SaveSettings;
    public static IReadOnlyList<EntranceTeleport> EntrancePoints => _entrancePoints;

    private void Awake()
    {
        if (StartOfRound.Instance == null)
            return;

        CRLibRandom = new System.Random(StartOfRound.Instance.randomMapSeed + 6969);
        StartOfRound.Instance.StartNewRoundEvent.AddListener(OnNewRoundStart);
        SaveSettings = new ES3Settings($"CRLib{GameNetworkManager.Instance.currentSaveFileName}", ES3.EncryptionType.None);
    }

    public IEnumerator Start()
    {
        yield return new WaitUntil(() => NetworkObject.IsSpawned);
        yield return new WaitUntil(() => GameNetworkManager.Instance.localPlayerController != null);
        if (IsHost || IsServer)
        {
            ProgressivePredicate.LoadAll(SaveSettings);
        }
        else
        {
            RequestProgressiveUnlockableStatesServerRpc(
                GameNetworkManager.Instance.localPlayerController,
                ProgressivePredicate.AllProgressiveItems
                    .Select(it => it.NetworkID)
                    .ToArray()
            );
        }
    }

    // to reduce the amount of network traffic that is sent
    [ServerRpc(RequireOwnership = false)]
    private void RequestProgressiveUnlockableStatesServerRpc(PlayerControllerReference requester, uint[] expectedOrder)
    {
        PlayerControllerB player = requester;
        DawnPlugin.Logger.LogDebug($"Sending states of progressive unlockables for player: '{player.playerUsername}'");
        bool[] values = new bool[expectedOrder.Length];

        for (int i = 0; i < expectedOrder.Length; i++)
        {
            uint unlockableNetworkId = expectedOrder[i];
            ProgressivePredicate? predicate = ProgressivePredicate.AllProgressiveItems.FirstOrDefault(it => { return it.NetworkID == unlockableNetworkId; });
            if (predicate)
            {
                values[i] = predicate!.IsUnlocked;
                Debuggers.Progressive?.Log($"set values[{i}] = {values[i]}");
            }
            else
            {
                DawnPlugin.Logger.LogError($"client requested progressive data status of a non-existing unlockable!!! (index: {i}, networkID: {unlockableNetworkId})");
                values[i] = false;
            }
        }

        ProgressiveUnlockableStateResponseClientRpc(values,
            new ClientRpcParams
            {
                Send =
                {
                    TargetClientIds = [player.OwnerClientId],
                },
            });
    }

    [ClientRpc]
    private void ProgressiveUnlockableStateResponseClientRpc(bool[] states, ClientRpcParams rpcParams = default)
    {
        ProgressivePredicate[] definitions = ProgressivePredicate.AllProgressiveItems.ToArray();
        for (int i = 0; i < definitions.Length; i++)
        {
            ProgressivePredicate predicate = definitions[i];
            predicate.SetFromServer(states[i]);
        }
    }

    internal void SaveCodeRebirthLibData()
    {
        if (!NetworkManager.Singleton.IsHost) return;
        ProgressivePredicate.SaveAll(SaveSettings);
    }

    internal static void ResetCodeRebirthLibData(ES3Settings saveSettings)
    {
        ES3.DeleteFile(saveSettings);
    }

    private void OnNewRoundStart()
    {
        _entrancePoints = FindObjectsByType<EntranceTeleport>(FindObjectsSortMode.InstanceID);
        foreach (EntranceTeleport? entrance in _entrancePoints)
        {
            if (!entrance.FindExitPoint())
            {
                DawnPlugin.Logger.LogError("Something went wrong in the generation of the fire exits! (ignorable if EntranceTeleportOptimisation is installed)");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void BroadcastDisplayTipServerRPC(HUDDisplayTip displayTip)
    {
        BroadcastDisplayTipClientRPC(displayTip);
    }

    [ClientRpc]
    private void BroadcastDisplayTipClientRPC(HUDDisplayTip displayTip)
    {
        HUDManager.Instance.DisplayTip(displayTip);
    }
}