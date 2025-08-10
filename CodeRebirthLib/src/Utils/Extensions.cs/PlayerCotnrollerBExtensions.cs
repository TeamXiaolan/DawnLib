using GameNetcodeStuff;

namespace CodeRebirthLib;

public static class PlayerControllerBExtensions
{
    public static bool IsLocalPlayer(this PlayerControllerB playerController)
    {
        return playerController == GameNetworkManager.Instance.localPlayerController;
    }
}