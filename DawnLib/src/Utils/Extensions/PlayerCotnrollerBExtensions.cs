using GameNetcodeStuff;

namespace CodeRebirthLib.Utils;

public static class PlayerControllerBExtensions
{
    public static bool IsLocalPlayer(this PlayerControllerB playerController)
    {
        return playerController == GameNetworkManager.Instance.localPlayerController;
    }
}