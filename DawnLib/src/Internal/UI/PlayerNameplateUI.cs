using GameNetcodeStuff;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dawn.Internal;
public class PlayerNameplateUI : MonoBehaviour
{
    [SerializeField] RawImage _image;
    [SerializeField] TMP_Text _usernameText;

    public Color TextColor
    {
        get => _usernameText.color;
        set => _usernameText.color = value;
    }

    public void Setup(PlayerControllerB player)
    {
        try
        {
            HUDManager.FillImageWithSteamProfile(_image, player.playerSteamId);
        }
        catch (System.Exception e)
        {
            DawnPlugin.Logger.LogDebug($"Hiding error for `PlayerNameplateUI.Setup` because it's like a false positive.");
            DawnPlugin.Logger.LogDebug($"Failed to set up player nameplate UI for player {player.playerUsername}: {e.Message}");
        }
        _usernameText.text = player.playerUsername;
    }
}