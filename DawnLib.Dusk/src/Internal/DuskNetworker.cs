using System.Collections;
using System.Linq;
using Dawn;
using Dawn.Internal;
using Dawn.Utils;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace Dusk.Internal;
public class DuskNetworker : NetworkSingleton<DuskNetworker>
{
    public IEnumerator Start()
    {
        yield return new WaitUntil(() => NetworkObject.IsSpawned);
        yield return new WaitUntil(() => GameNetworkManager.Instance.localPlayerController != null);
        DawnNetworker.Instance!.OnSave += SaveData;
        
        if (IsHost || IsServer)
        {
            ProgressivePredicate.LoadAll(DawnLib.GetCurrentContract()!);
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

    [ServerRpc(RequireOwnership = false)]
    internal void TriggerAchievementServerRpc(NamespacedKey namespacedKey)
    {
        TriggerAchievementClientRpc(namespacedKey);
    }

    [ClientRpc]
    private void TriggerAchievementClientRpc(NamespacedKey namespacedKey)
    {
        DuskModContent.Achievements[namespacedKey.AsTyped<DuskAchievementDefinition>()].TryCompleteFromServer();
    }

    internal void SaveData()
    {
        if (!IsHost) return;
        ProgressivePredicate.SaveAll(DawnLib.GetCurrentContract()!);
    }
}