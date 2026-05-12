using GameNetcodeStuff;
using Steamworks;
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

    public async void Setup(PlayerControllerB player)
    {
        try
        {
            Steamworks.Data.Image? steamProfilePicture = await SteamFriends.GetLargeAvatarAsync(player.playerSteamId);
            _image.texture = HUDManager.GetTextureFromImage(steamProfilePicture);
        }
        catch (System.Exception e)
        {
            DawnPlugin.Logger.LogDebug($"Hiding error for `PlayerNameplateUI.Setup` because it's like a false positive.");
            DawnPlugin.Logger.LogDebug($"Failed to set up player nameplate UI for player {player.playerUsername}: {e.Message}");
        }
        _usernameText.text = player.playerUsername;
    }
}