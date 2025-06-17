using CodeRebirthLib.Util;
using GameNetcodeStuff;
using System.Collections.Generic;

namespace CodeRebirthLib.Patches;
static class EnemyAIPatch
{

    public static void Init()
    {
        On.EnemyAI.Start += EnemyAI_Start;
        On.EnemyAI.HitEnemy += EnemyAI_HitEnemy;
        On.EnemyAI.OnDestroy += EnemyAI_OnDestroy;
    }

    private static void EnemyAI_OnDestroy(On.EnemyAI.orig_OnDestroy orig, EnemyAI self)
    {
        orig(self);
        CodeRebirthLibNetworker.ExtraEnemyDataDict.Remove(self);
    }

    private static void EnemyAI_Start(On.EnemyAI.orig_Start orig, EnemyAI self)
    {
        orig(self);
        ExtraEnemyData extraEnemyData = self.gameObject.AddComponent<ExtraEnemyData>();
        extraEnemyData.enemyAI = self;
        CodeRebirthLibNetworker.ExtraEnemyDataDict.Add(self, extraEnemyData);
    }

    private static void EnemyAI_HitEnemy(On.EnemyAI.orig_HitEnemy orig, EnemyAI self, int force, PlayerControllerB? playerWhoHit, bool playHitSFX, int hitID)
    {
        if (CodeRebirthLibNetworker.ExtraEnemyDataDict.TryGetValue(self, out ExtraEnemyData extraEnemyData))
        {
            if (playerWhoHit != null)
            {
                extraEnemyData.playerThatLastHit = playerWhoHit;
            }
            if (!self.isEnemyDead && self.enemyHP - force <= 0 && playerWhoHit != null)
            {
                extraEnemyData.enemyKilledByPlayer = true;
            }
        }

        orig(self, force, playerWhoHit, playHitSFX, hitID);
    }
}