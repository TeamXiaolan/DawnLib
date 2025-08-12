using System.Linq;
using CodeRebirthLib.CRMod;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeRebirthLib;
public class UnlockProgressiveObject : NetworkBehaviour // TODO, a better way to do the progressive stuff
{
    [FormerlySerializedAs("interactTrigger")]
    [SerializeField]
    private InteractTrigger _interactTrigger = null!;

    private void Start()
    {
        _interactTrigger.onInteract.AddListener(OnInteract);
    }

    private void OnInteract(PlayerControllerB player)
    {
        if (player != GameNetworkManager.Instance.localPlayerController || (player.currentlyHeldObjectServer is not UnlockableUpgradeScrap && player.currentlyHeldObjectServer is not ItemUpgradeScrap)) return;
        UnlockShipUpgradeServerRpc(player);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UnlockShipUpgradeServerRpc(PlayerControllerReference reference)
    {
        UnlockShipUpgradeClientRpc(reference);
    }

    [ClientRpc]
    private void UnlockShipUpgradeClientRpc(PlayerControllerReference reference)
    {
        PlayerControllerB player = reference; // implict cast
        if (player.currentlyHeldObjectServer is UnlockableUpgradeScrap unlockableUpgradeScrap)
        {
            ProgressiveUnlockData unlockData = ProgressiveUnlockableHandler.AllProgressiveUnlockables
                .First(it => it.Definition.UnlockableItem == unlockableUpgradeScrap.CRUnlockableReference.Resolve().UnlockableItem);
            unlockData.Unlock(
                new HUDDisplayTip(
                    "Assembled Parts",
                    $"Congratulations on finding the parts, Unlocked {unlockData.OriginalName}."
                )
            );
            if (unlockableUpgradeScrap.IsOwner) player.DespawnHeldObject();
        }
        else if (player.currentlyHeldObjectServer is ItemUpgradeScrap itemUpgradeScrap)
        {
            ProgressiveItemData itemData = ProgressiveItemHandler.AllProgressiveItems
                .First(it => it.Definition.Item == itemUpgradeScrap.CRItemReference.Resolve().Item);
            itemData.Unlock(
                new HUDDisplayTip(
                    "Assembled Parts",
                    $"Congratulations on finding the parts, Unlocked {itemData.OriginalName}."
                )
            );
            if (itemUpgradeScrap.IsOwner) player.DespawnHeldObject();
        }
        else
        {
            CodeRebirthLibPlugin.Logger.LogError("UnlockableUpgradeScrap is null, how did you even get here????");
        }
    }
}