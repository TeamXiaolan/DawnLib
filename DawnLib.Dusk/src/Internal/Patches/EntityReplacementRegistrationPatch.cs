using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using HarmonyLib;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Random = System.Random;

namespace Dusk.Internal;

static class EntityReplacementRegistrationPatch
{
    private static readonly NamespacedKey Key = NamespacedKey.From("dawn_lib", "entity_replacements");

    private static Random? replacementRandom = null;

    internal static void Init()
    {
        LethalContent.Enemies.BeforeFreeze += RegisterEnemyReplacements;
        using (new DetourContext(priority: int.MaxValue))
        {
            On.EnemyAI.Start += ReplaceEnemyEntity;
        }
        On.StartOfRound.SetShipReadyToLand += (orig, self) =>
        {
            replacementRandom = null;
            orig(self);
        };

        _ = new Hook(AccessTools.DeclaredMethod(typeof(EnemyAINestSpawnObject), "Awake"), OnNestSpawnAwake);
    }

    private static void OnNestSpawnAwake(RuntimeILReferenceBag.FastDelegateInvokers.Action<EnemyAINestSpawnObject> orig, EnemyAINestSpawnObject self)
    {
        if (!self.enemyType.GetDawnInfo().CustomData.TryGet(Key, out List<DuskEnemyReplacementDefinition>? replacements))
        {
            orig(self);
            return;
        }

        DawnMoonInfo currentMoon = RoundManager.Instance.currentLevel.GetDawnInfo();

        int? totalWeight = replacements.Sum(it => it.Weights.GetFor(currentMoon));
        if (totalWeight == null)
        {
            return;
        }

        if (replacementRandom == null)
        {
            replacementRandom = new Random(StartOfRound.Instance.randomMapSeed + 234780);
        }
        
        int chosenWeight = replacementRandom.Next(0, totalWeight.Value);
        foreach (DuskEnemyReplacementDefinition replacement in replacements)
        {
            chosenWeight -= replacement.Weights.GetFor(currentMoon) ?? 0;
            if(chosenWeight > 0)
                continue;

            replacement.ApplyNest(self);
        }
        orig(self);
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

        DawnMoonInfo currentMoon = RoundManager.Instance.currentLevel.GetDawnInfo();

        int? totalWeight = replacements.Sum(it => it.Weights.GetFor(currentMoon));
        if (totalWeight == null)
        {
            return;
        }

        if (replacementRandom == null)
        {
            replacementRandom = new Random(StartOfRound.Instance.randomMapSeed + 234780);
        }
        
        int chosenWeight = replacementRandom.Next(0, totalWeight.Value);
        foreach (DuskEnemyReplacementDefinition replacement in replacements)
        {
            chosenWeight -= replacement.Weights.GetFor(currentMoon) ?? 0;
            if(chosenWeight > 0)
                continue;

            replacement.Apply(self);
        }

        
        orig(self);
    }
}