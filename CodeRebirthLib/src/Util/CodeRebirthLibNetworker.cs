using System.Collections.Generic;
using System.Linq;
using CodeRebirthLib.ContentManagement.Unlockables;
using CodeRebirthLib.ContentManagement.Unlockables.Progressive;
using CodeRebirthLib.Extensions;
using GameNetcodeStuff;
using Mono.Cecil.Cil;
using Unity.Netcode;
using UnityEngine;

namespace CodeRebirthLib.Util;
public class CodeRebirthLibNetworker : NetworkSingleton<CodeRebirthLibNetworker>
{
    internal static EntranceTeleport[] _entrancePoints = [];
    public static IReadOnlyList<EntranceTeleport> EntrancePoints => _entrancePoints;
    internal ES3Settings SaveSettings;
    internal System.Random CRLibRandom = new();
    
    private void Awake()
    {
        if (StartOfRound.Instance == null)
            return;

        CRLibRandom = new System.Random(StartOfRound.Instance.randomMapSeed + 6969);
        StartOfRound.Instance.StartNewRoundEvent.AddListener(OnNewRoundStart);
        SaveSettings = new($"CRLib{GameNetworkManager.Instance.currentSaveFileName}", ES3.EncryptionType.None);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsHost || IsServer)
        {
            ProgressiveUnlockableHandler.LoadAll(SaveSettings);
        }
        else
        {
            RequestProgressiveUnlockableStatesServerRPC(
                GameNetworkManager.Instance.localPlayerController,
                CRMod.AllUnlockables()
                    .Where(it => it.ProgressiveData != null)
                    .Select(it => it.ProgressiveData!.networkID)
                    .ToArray()
            );
        }
    }

    // to reduce the amount of network traffic that is sent
    [ServerRpc(RequireOwnership = false)]
    void RequestProgressiveUnlockableStatesServerRPC(PlayerControllerReference requester, uint[] expectedOrder)
    {
        PlayerControllerB player = requester;
        CodeRebirthLibPlugin.Logger.LogInfo($"Sending states of progressive unlockables for player: '{player.playerUsername}'");
        bool[] values = new bool[expectedOrder.Length];

        for (int i = 0; i < expectedOrder.Length; i++)
        {
            uint unlockableNetworkId = expectedOrder[i];
            CRUnlockableDefinition? definition = CRMod.AllUnlockables().FirstOrDefault(it => it.ProgressiveData!.networkID == unlockableNetworkId);
            if (definition)
            {
                values[i] = definition!.ProgressiveData!.IsUnlocked;
                CodeRebirthLibPlugin.ExtendedLogging($"set values[{i}] = {values[i]}");
            }
            else
            {
                CodeRebirthLibPlugin.Logger.LogError($"client requested progressive data status of a non-existing unlockable!!! (index: {i}, networkID: {unlockableNetworkId})");
                values[i] = false;
            }
        }
        
        ProgressiveUnlockableStateResponseClientRPC(values, new ClientRpcParams() {
            Send = {
                TargetClientIds = [player.OwnerClientId]
            }
        });
    }

    [ClientRpc]
    void ProgressiveUnlockableStateResponseClientRPC(bool[] states, ClientRpcParams rpcParams = default)
    {
        CRUnlockableDefinition[] definitions = CRMod.AllUnlockables().Where(it => it.ProgressiveData != null).ToArray();
        for(int i = 0; i < definitions.Length; i++)
        {
            CRUnlockableDefinition definition = definitions[i];
            CodeRebirthLibPlugin.ExtendedLogging($"setting state of {definition.UnlockableItemDef.unlockable.unlockableName} to {states[i]}. (index: {i}, networkID: {definition.ProgressiveData!.networkID})");
            definition.ProgressiveData!.SetFromServer(states[i]);
        }
    }

    internal void SaveCodeRebirthLibData()
    {
        if (!NetworkManager.Singleton.IsHost) return;
        ProgressiveUnlockableHandler.SaveAll(SaveSettings);
    }

    internal static void ResetCodeRebirthLibData(ES3Settings saveSettings)
    {
        ES3.DeleteFile(saveSettings);
    }

    private void OnNewRoundStart()
    {
        _entrancePoints = FindObjectsByType<EntranceTeleport>(FindObjectsSortMode.InstanceID);
        foreach (var entrance in _entrancePoints)
        {
            if (!entrance.FindExitPoint())
            {
                CodeRebirthLibPlugin.Logger.LogError("Something went wrong in the generation of the fire exits! (ignorable if EntranceTeleportOptimisation is installed)");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    internal void BroadcastDisplayTipServerRPC(HUDDisplayTip displayTip)
    {
        BroadcastDisplayTipClientRPC(displayTip);
    }

    [ClientRpc]
    void BroadcastDisplayTipClientRPC(HUDDisplayTip displayTip)
    {
        HUDManager.Instance.DisplayTip(displayTip);
    }
}