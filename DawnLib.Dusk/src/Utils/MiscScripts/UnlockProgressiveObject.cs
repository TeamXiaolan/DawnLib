using Dawn;
using Dawn.Utils;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dusk.Utils;
public class UnlockProgressiveObject : NetworkBehaviour
{
    [FormerlySerializedAs("interactTrigger")]
    [SerializeField]
    private InteractTrigger _interactTrigger = null!;

    [SerializeField]
    [Tooltip("the word ProgressiveName is replaced with the Progressive's name that was just unlocked.")]
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
            DawnUnlockableItemInfo definition = unlockableUpgradeScrap.UnlockableReference.Resolve();
            if (player.currentlyHeldObjectServer.IsOwner)
            {
                player.DespawnHeldObject();
            }

            if (definition.PurchasePredicate is not ProgressivePredicate progressive)
            {
                DawnPlugin.Logger.LogError($"{definition.UnlockableItem.unlockableName} does not have a Progressive Predicate, yet is trying to be unlocked like one.");
                return;
            }

            _displayTip.Header.Replace("ProgressiveName", definition.UnlockableItem.unlockableName, true, System.Globalization.CultureInfo.InvariantCulture);
            _displayTip.Body.Replace("ProgressiveName", definition.UnlockableItem.unlockableName, true, System.Globalization.CultureInfo.InvariantCulture);
            progressive.Unlock(_displayTip);
        }
        else if (player.currentlyHeldObjectServer is ItemUpgradeScrap itemUpgradeScrap)
        {
            DawnItemInfo definition = itemUpgradeScrap.ItemReference.Resolve();
            if (player.currentlyHeldObjectServer.IsOwner)
            {
                player.DespawnHeldObject();
            }

            if (definition.ShopInfo == null)
            {
                DawnPlugin.Logger.LogError($"{definition.Item.itemName} is not a shop item. It can not be progressively unlocked.");
                return;
            }

            if (definition.ShopInfo.PurchasePredicate is not ProgressivePredicate progressive)
            {
                DawnPlugin.Logger.LogError($"{definition.Item.itemName} does not have a Progressive Predicate, yet is trying to be unlocked like one.");
                return;
            }

            _displayTip.Header.Replace("ProgressiveName", definition.Item.itemName, true, System.Globalization.CultureInfo.InvariantCulture);
            _displayTip.Body.Replace("ProgressiveName", definition.Item.itemName, true, System.Globalization.CultureInfo.InvariantCulture);
            progressive.Unlock(_displayTip);
        }
        else
        {
            DawnPlugin.Logger.LogError("UnlockableUpgradeScrap is null, how did you even get here????");
        }
    }
}