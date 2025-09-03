using System.Linq;
using Dawn.Dusk;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dawn.Utils;
public class UnlockProgressiveObject : NetworkBehaviour
{
    [FormerlySerializedAs("interactTrigger")]
    [SerializeField]
    private InteractTrigger _interactTrigger = null!;

    [SerializeField]
    private HUDDisplayTip _displayTip;
    
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
            CRUnlockableItemInfo definition = unlockableUpgradeScrap.CRUnlockableReference.Resolve();
            if (player.currentlyHeldObjectServer.IsOwner) player.DespawnHeldObject();

            if (definition.PurchasePredicate is not ProgressivePredicate progressive)
            {
                CodeRebirthLibPlugin.Logger.LogError($"{definition.UnlockableItem.unlockableName} does not have a Progressive Predicate, yet is trying to be unlocked like one.");
                return;
            }
            progressive.Unlock(_displayTip);
        }
        else if (player.currentlyHeldObjectServer is ItemUpgradeScrap itemUpgradeScrap)
        {
            CRItemInfo definition = itemUpgradeScrap.CRItemReference.Resolve();
            if (player.currentlyHeldObjectServer.IsOwner) player.DespawnHeldObject();

            if (definition.ShopInfo == null)
            {
                CodeRebirthLibPlugin.Logger.LogError($"{definition.Item.itemName} is not a shop item. It can not be progressively unlocked.");
                return;
            }
            
            if (definition.ShopInfo.PurchasePredicate is not ProgressivePredicate progressive)
            {
                CodeRebirthLibPlugin.Logger.LogError($"{definition.Item.itemName} does not have a Progressive Predicate, yet is trying to be unlocked like one.");
                return;
            }
            progressive.Unlock(_displayTip);
        }
        else
        {
            CodeRebirthLibPlugin.Logger.LogError("UnlockableUpgradeScrap is null, how did you even get here????");
        }
    }
}