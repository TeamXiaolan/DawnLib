using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dawn;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace Dusk.Internal;

static class EntityReplacementRegistrationPatch
{
    internal static void Init()
    {
        using (new DetourContext(priority: int.MaxValue))
        {
            On.EnemyAI.Start += ReplaceEnemyEntity;
        }
    }

    private static void ReplaceEnemyEntity(On.EnemyAI.orig_Start orig, EnemyAI self)
    {
        List<DuskEnemyReplacementDefinition> enemyReplacements = DuskModContent.EntityReplacements.Values.Where(enemyReplacement => enemyReplacement is DuskEnemyReplacementDefinition enemyReplacementDefinition).Cast<DuskEnemyReplacementDefinition>().ToList();
        List<DuskEnemyReplacementDefinition> validEnemyReplacements = enemyReplacements.Where(enemyReplacement => enemyReplacement.Key == self.enemyType.GetDawnInfo().Key).ToList();
        if (validEnemyReplacements.Count <= 0)
        {
            orig(self);
            return;
        }

        DuskEnemyReplacementDefinition pickedEnemyReplacement = validEnemyReplacements[0]; // TODO
        pickedEnemyReplacement.Apply(self);
        orig(self);
    }
}