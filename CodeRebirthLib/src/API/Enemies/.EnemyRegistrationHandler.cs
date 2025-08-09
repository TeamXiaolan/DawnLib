using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CodeRebirthLib;
static class EnemyRegistrationHandler
{
    internal static void Init()
    {
        On.StartOfRound.Awake += RegisterEnemies;
    }

    private static void RegisterEnemies(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        foreach (SelectableLevel level in self.levels)
        {
            foreach (CREnemyInfo enemyInfo in LethalContent.Enemies.Values)
            {
                if (enemyInfo.Key.IsVanilla())
                    continue; // also ensure not to register vanilla stuff again

                SpawnableEnemyWithRarity spawnDef = new()
                {
                    enemyType = enemyInfo.Enemy,
                    rarity = 0 // is 0 right?
                };

                // todo: xu you talked about wanting to register one enemy as daytime/outside/inside but this only registers as one?
                // todo: I think we gotta duplicate the SO's cuz of the daytime outside fields
                if (enemyInfo.Enemy.isDaytimeEnemy && enemyInfo.Enemy.isOutsideEnemy)
                {
                    level.DaytimeEnemies.Add(spawnDef);
                }
                else if (enemyInfo.Enemy.isOutsideEnemy)
                {
                    level.OutsideEnemies.Add(spawnDef);
                }
                else
                {
                    level.Enemies.Add(spawnDef);
                }
            }
        }

        // then, before freezing registry, add vanilla content
        if (!LethalContent.Enemies.IsFrozen) // effectively check for a lobby reload
        {
            // todo?
        }

        LethalContent.Enemies.Freeze();
        orig(self);
    }
}