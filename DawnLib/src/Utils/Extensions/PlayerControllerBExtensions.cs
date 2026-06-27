using System;
using GameNetcodeStuff;

namespace Dawn.Utils;

public static class PlayerControllerBExtensions
{
    extension(PlayerControllerB playerControllerB)
    {
        public bool IsLocalPlayer { get => playerControllerB == GameNetworkManager.Instance.localPlayerController; }
    }

    [Obsolete("Use PlayerControllerB.IsLocalPlayer instead")]
    public static bool IsLocalPlayer(this PlayerControllerB playerController)
    {
        return playerController == GameNetworkManager.Instance.localPlayerController;
    }
}