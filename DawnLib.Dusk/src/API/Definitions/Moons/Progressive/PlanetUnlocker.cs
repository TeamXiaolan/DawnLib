using System.Collections;
using Dawn;
using Dawn.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dusk;
public class PlanetUnlocker : GrabbableObject
{
    [Header("Planet Unlocker Settings")]
    [SerializeReference] private DuskMoonReference _moonReference;
    [SerializeField] private bool _consumeOnUnlock = true;
    [FormerlySerializedAs("_audio")] [SerializeField, Tooltip("Leave empty to have no audio")]
    private AudioSource _unlockAudio;
    
    [Header("Notification Settings")]
    [SerializeField] private bool _showDisplayTip;
    [SerializeField] private HUDDisplayTip _displayTip;

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);
        playerHeldBy.inSpecialInteractAnimation = true;

        if (!TryUnlock()) // failed to unlock
        {
            HUDManager.Instance.DisplayTip(new HUDDisplayTip(
                "Error",
                $"Coordinates to {_moonReference.Key.Key} could not be verified, Cancelling.",
                HUDDisplayTip.AlertType.Warning
            ));
        }

        if(_unlockAudio)
            _unlockAudio.Play();

        StartCoroutine(WaitToDespawn());
    }

    bool TryUnlock()
    {
        if (_moonReference.TryResolve(out DawnMoonInfo moonInfo))
        {
            if (moonInfo.DawnPurchaseInfo.PurchasePredicate is not ProgressivePredicate progressive)
            {
                DuskPlugin.Logger.LogError($"'{_moonReference.Key}' does not use a ProgressivePredicate");
                return false;
            }

            progressive.Unlock(_showDisplayTip ? _displayTip : null);
            return true;
        }
        else
        {
            DuskPlugin.Logger.LogError($"Couldn't resolve reference to '{_moonReference.Key}'. Is the bundle loaded?");
            return false;
        }
    }
    
    private IEnumerator WaitToDespawn()
    {
        if (_unlockAudio && _unlockAudio.clip != null)
        {
            yield return new WaitForSeconds(_unlockAudio.clip.length);
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }
        playerHeldBy.inSpecialInteractAnimation = false;
        if (!playerHeldBy.IsLocalPlayer())
            yield break;

        if(_consumeOnUnlock)
            playerHeldBy.DespawnHeldObject();
    }
}