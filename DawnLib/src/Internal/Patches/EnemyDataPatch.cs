using GameNetcodeStuff;
using Dawn.Utils;

namespace Dawn.Internal;

static class EnemyDataPatch
{
    internal static void Init()
    {
        On.EnemyAI.Start += CreateAdditionalEnemyData;
        On.EnemyAI.HitEnemy += HandleEnemyDeathData;
    }

    private static void HandleEnemyDeathData(On.EnemyAI.orig_HitEnemy orig, EnemyAI self, int force, PlayerControllerB playerWhoHit, bool playHitSFX, int hitID)
    {
        DawnEnemyAdditionalData data = DawnEnemyAdditionalData.CreateOrGet(self);
        ExtraEnemyEvents events = null;
        ExtraEnemyEvents.eventListeners.TryGetValue(self, out events);

        if (playerWhoHit)
        {
            data.PlayerThatLastHit = playerWhoHit;
        }

        if (!self.isEnemyDead && self.enemyHP - force <= 0)
        {
            if (playerWhoHit != null)
            {
                data.KilledByPlayer = true;
                if (events) events!.onKilledByPlayer.Invoke();
            }
            if (events) events!.onKilled.Invoke();
        }

        orig(self, force, playerWhoHit, playHitSFX, hitID);
    }

    private static void CreateAdditionalEnemyData(On.EnemyAI.orig_Start orig, EnemyAI self)
    {
        orig(self);
        DawnEnemyAdditionalData.CreateOrGet(self);
    }
}