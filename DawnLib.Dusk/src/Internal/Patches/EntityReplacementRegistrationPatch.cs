using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Dawn;
using Dawn.Internal;
using Dawn.Preloader.Interfaces;
using HarmonyLib;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using UnityEngine;
using OpCodes = Mono.Cecil.Cil.OpCodes;
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
            On.EnemyAI.UseNestSpawnObject += ReplaceEnemyEntityUsingNest;
        }

        On.StartOfRound.SetShipReadyToLand += (orig, self) =>
        {
            replacementRandom = null;
            orig(self);
        };

        _ = new Hook(AccessTools.DeclaredMethod(typeof(EnemyAINestSpawnObject), "Awake"), OnNestSpawnAwake);

        // this isn't great, but i don't know a better way to do it?
        // could maybe do some analysis on the game and then source generate this?
        DuskPlugin.Logger.LogInfo("Running transpiler 'DynamicallyReplaceAudioClips', this transpiler runs on a lot of functions, so this may take a second!");
        IL.EnemyAI.HitEnemy += DynamicallyReplaceAudioClips;
        IL.EnemyAI.SetEnemyStunned += DynamicallyReplaceAudioClips;

        IL.BaboonBirdAI.killPlayerAnimation += DynamicallyReplaceAudioClips;
        IL.BaboonBirdAI.OnCollideWithEnemy += DynamicallyReplaceAudioClips;
        IL.BaboonBirdAI.OnCollideWithPlayer += DynamicallyReplaceAudioClips;
        IL.BaboonBirdAI.Update += DynamicallyReplaceAudioClips;

        IL.BushWolfEnemy.OnCollideWithEnemy += DynamicallyReplaceAudioClips;

        IL.ButlerEnemyAI.ButlerBlowUpAndPop += DynamicallyReplaceAudioClips;
        IL.ButlerEnemyAI.StabPlayerClientRpc += DynamicallyReplaceAudioClips;
        IL.ButlerEnemyAI.Update += DynamicallyReplaceAudioClips;

        IL.DocileLocustBeesAI.DaytimeEnemyLeave += DynamicallyReplaceAudioClips;
        IL.DocileLocustBeesAI.Update += DynamicallyReplaceAudioClips;

        IL.DoublewingAI.Update += DynamicallyReplaceAudioClips;

        IL.FlowerSnakeEnemy.MakeChuckleClientRpc += DynamicallyReplaceAudioClips;
        IL.FlowerSnakeEnemy.SetClingToPlayer += DynamicallyReplaceAudioClips;
        IL.FlowerSnakeEnemy.SetFlappingLocalClient += DynamicallyReplaceAudioClips;
        IL.FlowerSnakeEnemy.Start += DynamicallyReplaceAudioClips;
        IL.FlowerSnakeEnemy.StartLeapOnLocalClient += DynamicallyReplaceAudioClips;
        IL.FlowerSnakeEnemy.StopClingingOnLocalClient += DynamicallyReplaceAudioClips;
        IL.FlowerSnakeEnemy.StopLeapOnLocalClient += DynamicallyReplaceAudioClips;

        IL.MaskedPlayerEnemy.killAnimation += DynamicallyReplaceAudioClips;
        IL.MaskedPlayerEnemy.SetMaskGlow += DynamicallyReplaceAudioClips;

        IL.NutcrackerEnemyAI.HitEnemy += DynamicallyReplaceAudioClips;
        IL.NutcrackerEnemyAI.ReloadGun += DynamicallyReplaceAudioClips;
        IL.NutcrackerEnemyAI.Update += DynamicallyReplaceAudioClips;

        IL.RadMechAI.ChangeBroadcastClipClientRpc += DynamicallyReplaceAudioClips;
        IL.RadMechAI.LateUpdate += DynamicallyReplaceAudioClips;
        IL.RadMechAI.Stomp += DynamicallyReplaceAudioClips;

        IL.RedLocustBees.BeesZap += DynamicallyReplaceAudioClips;
        IL.RedLocustBees.DaytimeEnemyLeave += DynamicallyReplaceAudioClips;
        DuskPlugin.Logger.LogInfo("Done 'DynamicallyReplaceAudioClips' patching!");
    }

    private static void ReplaceEnemyEntityUsingNest(On.EnemyAI.orig_UseNestSpawnObject orig, EnemyAI self, EnemyAINestSpawnObject nestSpawnObject)
    {
        if (nestSpawnObject.HasNestReplacement())
        {
            DuskEnemyReplacementDefinition enemyReplacementDefinition = nestSpawnObject.GetNestReplacement()!;
            enemyReplacementDefinition.Apply(self);
        }
        orig(self, nestSpawnObject);
    }

    // note!!! this transpiler should only be used on enemy AIs!
    private static readonly Dictionary<string, Func<EnemyAI, AudioClip, AudioClip>> clipReplacerFunctions = new()
    {
        { nameof(EnemyType.hitBodySFX), GenerateAudioClipReplacer(it => it.HitBodySFX) },
        { nameof(EnemyType.hitEnemyVoiceSFX), GenerateAudioClipReplacer(it => it.HitEnemyVoiceSFX) },
        { nameof(EnemyType.stunSFX), GenerateAudioClipReplacer(it => it.StunSFX) }
    };

    private static void DynamicallyReplaceAudioClips(ILContext il)
    {
        Debuggers.Patching?.Log($"patching: {il.Method.Name} with {nameof(DynamicallyReplaceAudioClips)}. il count {il.Body.Instructions.Count}");
        ILCursor c = new ILCursor(il);

        // you probably need to do null/empty array checks as well
        // this could also probably be cleaned up significantly.

        // evil for loop. loop backwards so emitting doesn't fuck us over later
        for (c.Index = c.Instrs.Count - 1; c.Index -1 >= 0; c.Index--)
        {
            if (c.Next.OpCode != OpCodes.Ldfld)
                continue;

            if (c.Next.MatchLdfld<EnemyType>(nameof(EnemyType.audioClips)))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((EnemyAI self, AudioClip[] existingAudioClips) =>
                {
                    ICurrentEntityReplacement replacement = (ICurrentEntityReplacement)self;
                    if (replacement.CurrentEntityReplacement == null)
                    {
                        return existingAudioClips;
                    }
                    return ((DuskEnemyReplacementDefinition)replacement.CurrentEntityReplacement).AudioClips;
                });
                continue;
            }

            foreach ((string name, var replacer) in clipReplacerFunctions)
            {
                if (!c.Next.MatchLdfld<EnemyType>(name))
                    continue;

                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate(replacer);
                break;
            }
        }
    }

    static Func<EnemyAI, AudioClip, AudioClip> GenerateAudioClipReplacer(Func<DuskEnemyReplacementDefinition, AudioClip> generator)
    {
        return (self, existing) =>
        {
            ICurrentEntityReplacement replacement = (ICurrentEntityReplacement)self;
            if (replacement.CurrentEntityReplacement == null) 
                return existing;
            
            AudioClip replacedClip = generator((DuskEnemyReplacementDefinition)replacement.CurrentEntityReplacement);
            if (!replacedClip)
                return existing;

            return replacedClip;
        };
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

        replacementRandom ??= new Random(StartOfRound.Instance.randomMapSeed + 234780);
        
        int chosenWeight = replacementRandom.Next(0, totalWeight.Value);
        foreach (DuskEnemyReplacementDefinition replacement in replacements)
        {
            chosenWeight -= replacement.Weights.GetFor(currentMoon) ?? 0;
            if (chosenWeight > 0)
                continue;

            if(replacement.IsVanilla)
                break;
            
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
                if(!enemyInfo.CustomData.TryGet(Key, out List<DuskEnemyReplacementDefinition> list))
                {
                    DuskEnemyReplacementDefinition vanilla = ScriptableObject.CreateInstance<DuskEnemyReplacementDefinition>();
                    vanilla.IsVanilla = true;
                    vanilla.Register(null);
                    list.Add(vanilla);
                    enemyInfo.CustomData.Set(Key, list);
                }
                list.Add(enemyReplacementDefinition);
            }
        }
    }

    private static void ReplaceEnemyEntity(On.EnemyAI.orig_Start orig, EnemyAI self)
    {
        orig(self);
        if (!self.enemyType.GetDawnInfo().CustomData.TryGet(Key, out List<DuskEnemyReplacementDefinition>? replacements))
        {
            return;
        }

        if (self.HasEnemyReplacement())
        {
            return;
        }

        DawnMoonInfo currentMoon = RoundManager.Instance.currentLevel.GetDawnInfo();

        int? totalWeight = replacements.Sum(it => it.Weights.GetFor(currentMoon));
        if (totalWeight == null)
        {
            return;
        }

        replacementRandom ??= new Random(StartOfRound.Instance.randomMapSeed + 234780);
        
        int chosenWeight = replacementRandom.Next(0, totalWeight.Value);
        foreach (DuskEnemyReplacementDefinition replacement in replacements)
        {
            chosenWeight -= replacement.Weights.GetFor(currentMoon) ?? 0;
            if (chosenWeight > 0)
                continue;

            if(replacement.IsVanilla)
                break;
                    
            replacement.Apply(self);
        }
    }
}