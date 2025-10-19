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
        HUDManager.FillImageWithSteamProfile(_image, player.playerSteamId);
        _usernameText.text = player.playerUsername;
    }
}