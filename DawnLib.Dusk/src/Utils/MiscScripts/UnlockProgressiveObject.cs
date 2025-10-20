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
    private bool _failGuaranteed = false;

    [SerializeField]
    [Tooltip("the word ProgressiveName is replaced with the Progressive's name that was just unlocked.")]
    private HUDDisplayTip _successDisplayTip;

    [SerializeField]
    [Tooltip("the word ProgressiveName is replaced with the Progressive's name that failed to unlock.")]
    private HUDDisplayTip _failureDisplayTip;

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
            if (_failGuaranteed)
            {
                HUDManager.Instance.DisplayTip(_failureDisplayTip);
                return;
            }
            if (!unlockableUpgradeScrap.UnlockableReference.TryResolve(out DawnUnlockableItemInfo definition))
            {
                DawnPlugin.Logger.LogWarning($"Tried to unlock progressive unlockable upgrade for an unlockable that does not exist: {unlockableUpgradeScrap.UnlockableReference}");
                return;
            }

            if (player.currentlyHeldObjectServer.IsOwner)
            {
                player.DespawnHeldObject();
            }

            if (definition.DawnPurchaseInfo.PurchasePredicate is not ProgressivePredicate progressive)
            {
                DawnPlugin.Logger.LogError($"{definition.UnlockableItem.unlockableName} does not have a Progressive Predicate, yet is trying to be unlocked like one.");
                return;
            }

            string header = _successDisplayTip.Header.Replace("ProgressiveName", definition.UnlockableItem.unlockableName, true, System.Globalization.CultureInfo.InvariantCulture);
            string body = _successDisplayTip.Body.Replace("ProgressiveName", definition.UnlockableItem.unlockableName, true, System.Globalization.CultureInfo.InvariantCulture);
            HUDDisplayTip.AlertType alertType = _successDisplayTip.Type;
            _successDisplayTip = new HUDDisplayTip(header, body, alertType);
            progressive.Unlock(_successDisplayTip);
        }
        else if (player.currentlyHeldObjectServer is ItemUpgradeScrap itemUpgradeScrap)
        {
            if (_failGuaranteed)
            {
                HUDManager.Instance.DisplayTip(_failureDisplayTip);
                return;
            }
            if (!itemUpgradeScrap.ItemReference.TryResolve(out DawnItemInfo definition))
            {
                DawnPlugin.Logger.LogWarning($"Tried to unlock progressive item upgrade for an item that does not exist: {itemUpgradeScrap.ItemReference}");
                return;
            }

            if (player.currentlyHeldObjectServer.IsOwner)
            {
                player.DespawnHeldObject();
            }

            if (definition.ShopInfo == null)
            {
                DawnPlugin.Logger.LogError($"{definition.Item.itemName} is not a shop item. It can not be progressively unlocked.");
                return;
            }

            if (definition.ShopInfo.DawnPurchaseInfo.PurchasePredicate is not ProgressivePredicate progressive)
            {
                DawnPlugin.Logger.LogError($"{definition.Item.itemName} does not have a Progressive Predicate, yet is trying to be unlocked like one.");
                return;
            }

            string header = _successDisplayTip.Header.Replace("ProgressiveName", definition.Item.itemName, true, System.Globalization.CultureInfo.InvariantCulture);
            string body = _successDisplayTip.Body.Replace("ProgressiveName", definition.Item.itemName, true, System.Globalization.CultureInfo.InvariantCulture);
            HUDDisplayTip.AlertType alertType = _successDisplayTip.Type;
            _successDisplayTip = new HUDDisplayTip(header, body, alertType);
            progressive.Unlock(_successDisplayTip);
        }
        else if (player.currentlyHeldObjectServer is MoonProgressiveScrap moonProgressiveScrap)
        {
            if (_failGuaranteed)
            {
                HUDManager.Instance.DisplayTip(_failureDisplayTip);
                return;
            }
            if (!moonProgressiveScrap.MoonReference.TryResolve(out DawnMoonInfo definition))
            {
                DawnPlugin.Logger.LogWarning($"Tried to unlock progressive moon for a moon that does not exist: {moonProgressiveScrap.MoonReference}");
                return;
            }

            if (player.currentlyHeldObjectServer.IsOwner)
            {
                player.DespawnHeldObject();
            }

            if (definition.DawnPurchaseInfo.PurchasePredicate is not ProgressivePredicate progressive)
            {
                DawnPlugin.Logger.LogError($"{definition.GetNumberlessPlanetName()} does not have a Progressive Predicate, yet is trying to be unlocked like one.");
                return;
            }

            string header = _successDisplayTip.Header.Replace("ProgressiveName", definition.GetNumberlessPlanetName(), true, System.Globalization.CultureInfo.InvariantCulture);
            string body = _successDisplayTip.Body.Replace("ProgressiveName", definition.GetNumberlessPlanetName(), true, System.Globalization.CultureInfo.InvariantCulture);
            HUDDisplayTip.AlertType alertType = _successDisplayTip.Type;
            _successDisplayTip = new HUDDisplayTip(header, body, alertType);
            progressive.Unlock(_successDisplayTip);
        }
        else
        {
            DawnPlugin.Logger.LogError("UnlockableUpgradeScrap is null, how did you even get here????");
        }
    }
}