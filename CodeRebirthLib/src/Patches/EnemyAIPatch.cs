using CodeRebirthLib.Util;
using GameNetcodeStuff;
using System.Collections.Generic;
using CodeRebirthLib.ContentManagement.Enemies;

namespace CodeRebirthLib.Patches;
static class EnemyAIPatch
{

    public static void Init()
    {
        On.EnemyAI.Start += EnemyAI_Start;
        On.EnemyAI.HitEnemy += EnemyAI_HitEnemy;
    }

    private static void EnemyAI_Start(On.EnemyAI.orig_Start orig, EnemyAI self)
    {
        orig(self);
        CREnemyAdditionalData.CreateOrGet(self);
    }

    private static void EnemyAI_HitEnemy(On.EnemyAI.orig_HitEnemy orig, EnemyAI self, int force, PlayerControllerB? playerWhoHit, bool playHitSFX, int hitID)
    {
        CREnemyAdditionalData data = CREnemyAdditionalData.CreateOrGet(self);

        if (playerWhoHit)
        {
            data.PlayerThatLastHit = playerWhoHit;
        }
        
        if (!self.isEnemyDead && self.enemyHP - force <= 0 && playerWhoHit != null)
        {
            data.KilledByPlayer = true;
        }

        orig(self, force, playerWhoHit, playHitSFX, hitID);
    }
}