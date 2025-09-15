using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using MonoMod.RuntimeDetour;

namespace Dusk.Internal;

static class EntityReplacementRegistrationPatch
{
    private static readonly NamespacedKey Key = NamespacedKey.From("dawn_lib", "entity_replacements");

    internal static void Init()
    {
        LethalContent.Enemies.BeforeFreeze += RegisterEnemyReplacements;
        using (new DetourContext(priority: int.MaxValue))
        {
            On.EnemyAI.Start += ReplaceEnemyEntity;
        }
    }

    private static void RegisterEnemyReplacements()
    {
        foreach (DuskEntityReplacementDefinition entityReplacementDefinition in DuskModContent.EntityReplacements.Values)
        {
            if (entityReplacementDefinition is not DuskEnemyReplacementDefinition enemyReplacementDefinition)
                continue;

            if (LethalContent.Enemies.TryGetValue(enemyReplacementDefinition.EntityToReplaceKey, out DawnEnemyInfo enemyInfo))
            {
                enemyInfo.CustomData.GetOrCreateDefault<List<DuskEnemyReplacementDefinition>>(Key).Add(enemyReplacementDefinition);
            }
        }
    }

    private static void ReplaceEnemyEntity(On.EnemyAI.orig_Start orig, EnemyAI self)
    {;
        if (!self.enemyType.GetDawnInfo().CustomData.TryGet(Key, out List<DuskEnemyReplacementDefinition>? replacements))
        {
            orig(self);
            return;
        }

        DuskEnemyReplacementDefinition pickedEnemyReplacement = replacements[0]; // TODO rarity
        pickedEnemyReplacement.Apply(self);
        orig(self);
    }
}