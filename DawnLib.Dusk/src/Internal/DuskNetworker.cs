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
        DawnPlugin.Logger.LogDebug($"{nameof(DuskNetworker)} started.");
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
    private void RequestProgressiveUnlockableStatesServerRpc(PlayerControllerReference requester, ulong[] expectedOrder)
    {
        PlayerControllerB player = requester;
        DawnPlugin.Logger.LogDebug($"Sending states of progressive unlockables for player: '{player.playerUsername}'");
        bool[] unlockedStates = new bool[expectedOrder.Length];
        bool[] hiddenStates = new bool[expectedOrder.Length];

        for (int i = 0; i < expectedOrder.Length; i++)
        {
            ulong unlockableNetworkId = expectedOrder[i];
            ProgressivePredicate? predicate = ProgressivePredicate.AllProgressiveItems.FirstOrDefault(it => { return it.NetworkID == unlockableNetworkId; });
            if (predicate)
            {
                unlockedStates[i] = predicate.ProgressiveStates.IsUnlocked;
                hiddenStates[i] = predicate.ProgressiveStates.IsHidden;
                Debuggers.Progressive?.Log($"set unlockedStates[{i}] = {unlockedStates[i]}");
            }
            else
            {
                DawnPlugin.Logger.LogError($"client requested progressive data status of a non-existing unlockable!!! (index: {i}, networkID: {unlockableNetworkId})");
                unlockedStates[i] = false;
                hiddenStates[i] = false;
            }
        }

        ProgressiveUnlockableStateResponseClientRpc(unlockedStates, hiddenStates,
            new ClientRpcParams
            {
                Send =
                {
                    TargetClientIds = [player.OwnerClientId],
                },
            });
    }

    [ClientRpc]
    private void ProgressiveUnlockableStateResponseClientRpc(bool[] unlockedStates, bool[] hiddenStates, ClientRpcParams rpcParams = default)
    {
        ProgressivePredicate[] definitions = ProgressivePredicate.AllProgressiveItems.ToArray();
        for (int i = 0; i < definitions.Length; i++)
        {
            ProgressivePredicate predicate = definitions[i];
            predicate.SetFromServer(unlockedStates[i], hiddenStates[i]);
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

    [ServerRpc]
    internal void SyncVehicleDeliveredServerRpc(int lastVehicleDelivered)
    {
        SyncVehicleDeliveredClientRpc(lastVehicleDelivered);
    }

    [ClientRpc]
    private void SyncVehicleDeliveredClientRpc(int lastVehicleDelivered)
    {
        TerminalRefs.LastVehicleDelivered = lastVehicleDelivered;
    }

    [ServerRpc]
    internal void SyncVehicleIntoDropShipAnimationServerRpc(NetworkObjectReference netObjRef)
    {
        SyncVehicleIntoDropShipAnimationClientRpc(netObjRef);
    }

    [ClientRpc]
    private void SyncVehicleIntoDropShipAnimationClientRpc(NetworkObjectReference netObjRef)
    {
        if (netObjRef.TryGet(out NetworkObject netObj) && netObj.TryGetComponent(out VehicleBase vehicleBase))
        {
            vehicleBase.InDropShipAnimation = true;
        }
    }

    internal void SaveData()
    {
        if (!IsHost)
        {
            return;
        }

        ProgressivePredicate.SaveAll(DawnLib.GetCurrentContract()!);
    }

    [Rpc(SendTo.NotServer)]
    internal void SendClientEntityReplacementRpc(SkinSyncEntry skinSyncEntry)
    {
        NetworkObjectReference grabbableObjectRef = skinSyncEntry.GrabbableRef;
        ulong networkId = skinSyncEntry.NetworkID;
        if (!grabbableObjectRef.TryGet(out NetworkObject networkObject) || !networkObject.TryGetComponent(out GrabbableObject grabbableObject))
        {
            return;
        }

        if (!DuskModContent.EntityReplacements.TryGetReplacementByNetworkId(networkId, out DuskEntityReplacementDefinition? itemReplacement))
        {
            return;
        }

        if (itemReplacement is not DuskItemReplacementDefinition itemReplacementDefinition)
        {
            return;
        }

        StartOfRoundRefs.Instance.StartCoroutine(itemReplacementDefinition.Apply(grabbableObject));
    }

    [Rpc(SendTo.Server)]
    internal void SyncSkinDataRpc(ulong clientId)
    {
        GrabbableObject[] grabbableObjects = FindObjectsOfType<GrabbableObject>().Where(it => it.TryGetGrabbableObjectReplacement(out _)).ToArray();
        SkinSyncEntry[] entries = new SkinSyncEntry[grabbableObjects.Length];

        for (int i = 0; i < grabbableObjects.Length; i++)
        {
            GrabbableObject grabbable = grabbableObjects[i];
            grabbable.TryGetGrabbableObjectReplacement(out DuskItemReplacementDefinition? replacement);
            entries[i] = new SkinSyncEntry(new NetworkObjectReference(grabbable.NetworkObject), replacement!.Key.NetworkID);
        }

        InformClientOfSkinDataRpc(
            entries,
            RpcTarget.Single(clientId, RpcTargetUse.Temp)
        );
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void InformClientOfSkinDataRpc(SkinSyncEntry[] entries, RpcParams rpcParams = default)
    {
        foreach (SkinSyncEntry entry in entries)
        {
            if (!entry.GrabbableRef.TryGet(out NetworkObject networkObject))
            {
                DawnPlugin.Logger.LogWarning($"Failed to resolve NetworkObject for skin sync entry {entry.NetworkID}");
                continue;
            }

            if (!networkObject.TryGetComponent(out GrabbableObject grabbableObject))
            {
                DawnPlugin.Logger.LogWarning($"Resolved NetworkObject but no GrabbableObject for {entry.NetworkID}");
                continue;
            }

            if (!DuskModContent.EntityReplacements.TryGetReplacementByNetworkId(entry.NetworkID, out DuskEntityReplacementDefinition? replacement))
            {
                DawnPlugin.Logger.LogWarning($"Failed to resolve replacement by network id {entry.NetworkID}");
                continue;
            }

            if (replacement is not DuskItemReplacementDefinition itemReplacementDefinition)
            {
                DawnPlugin.Logger.LogWarning($"Replacement {entry.NetworkID} was not a DuskItemReplacementDefinition, Skin Name: {replacement.SkinName} | Name: {replacement.name} | Type: {replacement.GetType().Name}");
                continue;
            }

            StartOfRoundRefs.Instance.StartCoroutine(itemReplacementDefinition.Apply(grabbableObject));
        }
    }
}