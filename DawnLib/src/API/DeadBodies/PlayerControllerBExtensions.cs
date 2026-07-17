using GameNetcodeStuff;
using UnityEngine;

namespace Dawn;

public static partial class PlayerControllerBExtensions
{
    extension(PlayerControllerB playerControllerB)
    {
        public void KillPlayer(Vector3 bodyVelocity, bool spawnBody, CauseOfDeath causeOfDeath, NamespacedKey deathAnimationKey, Vector3 positionOffset, bool setOverrideDropItems)
        {
            if (!LethalContent.DeadBodies.TryGetValue(deathAnimationKey, out DawnDeadBodyInfo deadBodyInfo))
            {
                return;
            }

            playerControllerB.KillPlayer(bodyVelocity, spawnBody, causeOfDeath, deadBodyInfo.Index, positionOffset, setOverrideDropItems);
        }
    }
}