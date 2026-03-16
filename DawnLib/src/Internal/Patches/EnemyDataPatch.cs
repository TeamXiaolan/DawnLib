using GameNetcodeStuff;
using Dawn.Utils;
using UnityEngine;

namespace Dawn.Internal;

static class EnemyDataPatch
{
    internal static void Init()
    {
        On.EnemyAI.Start += CreateAdditionalEnemyData;
        On.EnemyAI.HitEnemy += HandleEnemyDeathData;
        On.EnemyAI.SetClientCalculatingAI += HandleClientCalculatingAI;
    }

    private static void HandleClientCalculatingAI(On.EnemyAI.orig_SetClientCalculatingAI orig, EnemyAI self, bool enable)
    {
        if (self.agent == null)
        {
            orig(self, enable);
            return;
        }

        bool agentEnabledState = self.agent.enabled;
        orig(self, enable);
        DawnEnemyAdditionalData data = DawnEnemyAdditionalData.CreateOrGet(self);
        if (data.SmartAgentNavigator != null && data.SmartAgentNavigator.CanTryToFlyToDestination && data.SmartAgentNavigator.pointToGo != Vector3.zero && data.SmartAgentNavigator.pointToStart != Vector3.zero)
        {
            self.agent.enabled = agentEnabledState;
        }
    }

    private static void HandleEnemyDeathData(On.EnemyAI.orig_HitEnemy orig, EnemyAI self, int force, PlayerControllerB playerWhoHit, bool playHitSFX, int hitID)
    {
        DawnEnemyAdditionalData data = DawnEnemyAdditionalData.CreateOrGet(self);
        ExtraEnemyEvents.eventListeners.TryGetValue(self, out ExtraEnemyEvents events);

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