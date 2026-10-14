using System.Collections;
using System.Linq;
using Dawn;
using Dawn.Internal;
using Dawn.Utils;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace Nightfall.Internal;

public class NightfallNetworker : NetworkSingleton<NightfallNetworker>
{
    public void Start()
    {
        DawnPlugin.Logger.LogDebug($"{nameof(NightfallNetworker)} started.");
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

        if (!NightfallModContent.EntityReplacements.TryGetReplacementByNetworkId(networkId, out NightfallEntityReplacementDefinition? itemReplacement))
        {
            return;
        }

        if (itemReplacement is not NightfallItemReplacementDefinition itemReplacementDefinition)
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
            grabbable.TryGetGrabbableObjectReplacement(out NightfallItemReplacementDefinition? replacement);
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

            if (!NightfallModContent.EntityReplacements.TryGetReplacementByNetworkId(entry.NetworkID, out NightfallEntityReplacementDefinition? replacement))
            {
                DawnPlugin.Logger.LogWarning($"Failed to resolve replacement by network id {entry.NetworkID}");
                continue;
            }

            if (replacement is not NightfallItemReplacementDefinition itemReplacementDefinition)
            {
                DawnPlugin.Logger.LogWarning($"Replacement {entry.NetworkID} was not a NightfallItemReplacementDefinition, Skin Name: {replacement.SkinName} | Name: {replacement.name} | Type: {replacement.GetType().Name}");
                continue;
            }

            StartOfRoundRefs.Instance.StartCoroutine(itemReplacementDefinition.Apply(grabbableObject));
        }
    }
}