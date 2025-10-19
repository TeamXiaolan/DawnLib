using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Dawn.Internal;
using Dawn.Interfaces;
using HarmonyLib;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using UnityEngine;
using OpCodes = Mono.Cecil.Cil.OpCodes;
using Random = System.Random;

namespace Dusk.Internal;

static class EntityReplacementRegistrationPatch
{
    internal static readonly NamespacedKey Key = NamespacedKey.From("dawn_lib", "entity_replacements");

    internal static Random? replacementRandom = null;

    internal static void Init()
    {
        LethalContent.Enemies.BeforeFreeze += RegisterEnemyReplacements;
        LethalContent.Items.BeforeFreeze += RegisterItemReplacements;
        LethalContent.Unlockables.BeforeFreeze += RegisterUnlockableReplacements;
        using (new DetourContext(priority: int.MaxValue))
        {
            On.StartOfRound.Awake += RegisterScenePlacedUnlockableReplacements;
            On.EnemyAI.Start += ReplaceEnemyEntity;
            On.GrabbableObject.Start += ReplaceGrabbableObject;
            On.EnemyAI.UseNestSpawnObject += ReplaceEnemyEntityUsingNest;
        }

        On.StartOfRound.SetShipReadyToLand += (orig, self) =>
        {
            replacementRandom = null;
            orig(self);
        };

        DawnPlugin.Hooks.Add(new Hook(AccessTools.DeclaredMethod(typeof(EnemyAINestSpawnObject), "Awake"), OnNestSpawnAwake));

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

    private static void RegisterScenePlacedUnlockableReplacements(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        AutoParentToShip[] autoParentToShips = GameObject.FindObjectsOfType<AutoParentToShip>(true);
        foreach (AutoParentToShip autoParentToShip in autoParentToShips)
        {
            if (autoParentToShip.gameObject.GetComponent<DuskUnlockable>())
            {
                DuskPlugin.Logger.LogWarning($"{autoParentToShip.gameObject.name} already has a DuskUnlockable component somehow.");
                continue;
            }
            autoParentToShip.gameObject.AddComponent<DuskUnlockable>();
        }
        orig(self);
    }

    private static void RegisterUnlockableReplacements()
    {
        foreach (DawnUnlockableItemInfo unlockableItemInfo in LethalContent.Unlockables.Values)
        {
            if (unlockableItemInfo.UnlockableItem.prefabObject == null)
                continue;

            if (unlockableItemInfo.UnlockableItem.prefabObject.GetComponent<DuskUnlockable>())
                continue;

            unlockableItemInfo.UnlockableItem.prefabObject.AddComponent<DuskUnlockable>();
        }

        foreach (DuskEntityReplacementDefinition entityReplacementDefinition in DuskModContent.EntityReplacements.Values)
        {
            if (entityReplacementDefinition is not DuskUnlockableReplacementDefinition unlockableReplacementDefinition)
                continue;

            if (LethalContent.Unlockables.TryGetValue(unlockableReplacementDefinition.EntityToReplaceKey, out DawnUnlockableItemInfo unlockableItemInfo))
            {
                if (!unlockableItemInfo.CustomData.TryGet(Key, out List<DuskUnlockableReplacementDefinition>? list))
                {
                    DuskUnlockableReplacementDefinition vanilla = ScriptableObject.CreateInstance<DuskUnlockableReplacementDefinition>();
                    vanilla.IsDefault = true;
                    vanilla.Register(null);
                    list = [vanilla];
                    unlockableItemInfo.CustomData.Set(Key, list);
                }
                list.Add(unlockableReplacementDefinition);
            }
        }
    }

    private static void RegisterItemReplacements()
    {
        foreach (DuskEntityReplacementDefinition entityReplacementDefinition in DuskModContent.EntityReplacements.Values)
        {
            if (entityReplacementDefinition is not DuskItemReplacementDefinition itemReplacementDefinition)
                continue;

            if (LethalContent.Items.TryGetValue(itemReplacementDefinition.EntityToReplaceKey, out DawnItemInfo itemInfo))
            {
                if (!itemInfo.CustomData.TryGet(Key, out List<DuskItemReplacementDefinition>? list))
                {
                    DuskItemReplacementDefinition vanilla = ScriptableObject.CreateInstance<DuskItemReplacementDefinition>();
                    vanilla.IsDefault = true;
                    vanilla.Register(null);
                    list = [vanilla];
                    itemInfo.CustomData.Set(Key, list);
                }
                list.Add(itemReplacementDefinition);
            }
        }
    }

    private static void ReplaceGrabbableObject(On.GrabbableObject.orig_Start orig, GrabbableObject self)
    {
        if (!self.itemProperties.HasDawnInfo())
        {
            orig(self);
            return;
        }

        if (!self.itemProperties.GetDawnInfo().CustomData.TryGet(Key, out List<DuskItemReplacementDefinition>? replacements))
        {
            orig(self);
            return;
        }

        foreach (DuskItemReplacementDefinition replacement in replacements.ToArray())
        {
            if (replacement.DatePredicate == null)
                continue;

            if (!replacement.DatePredicate.Evaluate())
            {
                replacements.Remove(replacement);
            }
        }

        if (self.HasGrabbableObjectReplacement())
        {
            orig(self);
            return;
        }

        // todo: save the current skin and try to restore it if this runs in orbit
        if (StartOfRound.Instance.inShipPhase)
        {
            orig(self);
            return;
        }

        DawnMoonInfo currentMoon = RoundManager.Instance.currentLevel.GetDawnInfo();

        int? totalWeight = replacements.Sum(it => it.Weights.GetFor(currentMoon));
        if (totalWeight == null)
        {
            orig(self);
            return;
        }

        replacementRandom ??= new Random(StartOfRound.Instance.randomMapSeed + 234780);

        int chosenWeight = replacementRandom.Next(0, totalWeight.Value);
        foreach (DuskItemReplacementDefinition replacement in replacements)
        {
            chosenWeight -= replacement.Weights.GetFor(currentMoon) ?? 0;
            if (chosenWeight > 0)
                continue;

            if (replacement.IsDefault)
                break;

            replacement.Apply(self);
        }
        orig(self);
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
    private static readonly Dictionary<string, Func<AudioClip, EnemyAI, AudioClip?>> clipReplacerFunctions = new()
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

        // evil for loop.
        for (; c.Index < c.Instrs.Count; c.Index++)
        {
            if (c.Next.OpCode != OpCodes.Ldfld)
                continue;

            if (c.Next.MatchLdfld<EnemyType>(nameof(EnemyType.audioClips)))
            {
                c.Index++;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<AudioClip[], EnemyAI, AudioClip[]>>((existingAudioClips, self) =>
                {
                    if (!self.HasEnemyReplacement())
                    {
                        return existingAudioClips;
                    }
                    return self.GetEnemyReplacement().AudioClips;
                });
                continue;
            }

            foreach ((string name, var replacer) in clipReplacerFunctions)
            {
                if (!c.Next.MatchLdfld<EnemyType>(name))
                    continue;

                c.Index++;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate(replacer);
                break;
            }
        }
    }

    static Func<AudioClip?, EnemyAI, AudioClip?> GenerateAudioClipReplacer(Func<DuskEnemyReplacementDefinition, AudioClip?> generator)
    {
        return (existing, self) =>
        {
            ICurrentEntityReplacement replacement = (ICurrentEntityReplacement)self;
            if (replacement.CurrentEntityReplacement == null)
                return existing;

            AudioClip? replacedClip = generator((DuskEnemyReplacementDefinition)replacement.CurrentEntityReplacement);
            if (!replacedClip)
                return existing;

            return replacedClip;
        };
    }

    private static void OnNestSpawnAwake(RuntimeILReferenceBag.FastDelegateInvokers.Action<EnemyAINestSpawnObject> orig, EnemyAINestSpawnObject self)
    {
        if (!self.enemyType.HasDawnInfo())
        {
            DuskPlugin.Logger.LogWarning($"Failed to replace enemy nest for '{self.enemyType.enemyName}', it doesn't have a dawn info! (there may be other problems)");
            orig(self);
            return;
        }

        if (!self.enemyType.GetDawnInfo().CustomData.TryGet(Key, out List<DuskEnemyReplacementDefinition>? replacements))
        {
            orig(self);
            return;
        }

        foreach (DuskEnemyReplacementDefinition replacement in replacements.ToArray())
        {
            if (replacement.DatePredicate == null)
                continue;

            if (!replacement.DatePredicate.Evaluate())
            {
                replacements.Remove(replacement);
            }
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

            if (replacement.IsDefault)
                break;

            replacement.ApplyNest(self);
        }
        orig(self);
    }

    private static void RegisterEnemyReplacements()
    {
        foreach (DuskEntityReplacementDefinition entityReplacementDefinition in DuskModContent.EntityReplacements.Values)
        {
            Debuggers.ReplaceThis?.Log($"1. Registering replacement for {entityReplacementDefinition.EntityToReplaceKey} with {entityReplacementDefinition.TypedKey}");
            if (entityReplacementDefinition is not DuskEnemyReplacementDefinition enemyReplacementDefinition)
                continue;

            if (LethalContent.Enemies.TryGetValue(enemyReplacementDefinition.EntityToReplaceKey, out DawnEnemyInfo enemyInfo))
            {
                Debuggers.Enemies?.Log($"2. Registering replacement success");
                if (!enemyInfo.CustomData.TryGet(Key, out List<DuskEnemyReplacementDefinition>? list))
                {
                    DuskEnemyReplacementDefinition defaultSkin = ScriptableObject.CreateInstance<DuskEnemyReplacementDefinition>();
                    defaultSkin.IsDefault = true;
                    defaultSkin.Register(null);
                    list = [defaultSkin];
                    enemyInfo.CustomData.Set(Key, list);
                }
                list.Add(enemyReplacementDefinition);
                continue;
            }
            Debuggers.Enemies?.Log($"3. Registering replacement failure");
        }
    }

    private static void ReplaceEnemyEntity(On.EnemyAI.orig_Start orig, EnemyAI self)
    {
        orig(self);
        if (!self.enemyType.HasDawnInfo())
        {
            DuskPlugin.Logger.LogWarning($"Failed to replace enemy entity for '{self.enemyType.enemyName}', it doesn't have a dawn info! (there may be other problems)");
            return;
        }

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

            if (replacement.IsDefault)
                break;

            replacement.Apply(self);
        }
    }
}