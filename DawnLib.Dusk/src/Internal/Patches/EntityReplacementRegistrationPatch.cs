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
        LethalContent.MapObjects.BeforeFreeze += RegisterMapObjectReplacements;
        using (new DetourContext(priority: int.MaxValue))
        {
            On.StartOfRound.Awake += RegisterScenePlacedUnlockableReplacements;
            On.EnemyAI.Start += ReplaceEnemyEntity;
            On.GrabbableObject.Start += ReplaceGrabbableObject;
            On.EnemyAI.UseNestSpawnObject += ReplaceEnemyEntityUsingNest;
        }

        using (new DetourContext(priority: -200))
        {
            On.Terminal.Awake += RegisterDuskUnlockables;
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

        DuskPlugin.Logger.LogInfo("Running transpiler 'DynamicallyReplaceItemProperties', this transpiler runs on a lot of functions, so this may take a second!");

        IL.DepositItemsDesk.PlaceItemOnCounter += DynamicallyReplaceItemProperties;
        IL.GameNetcodeStuff.PlayerControllerB.LateUpdate += DynamicallyReplaceItemProperties;
        IL.PlaceableObjectsSurface.itemPlacementPosition += DynamicallyReplaceItemProperties;
        IL.HUDManager.DisplayNewScrapFound += DynamicallyReplaceItemProperties;

        // IL.RoundManager.SpawnScrapInLevel += DynamicallyReplaceItemProperties; - this does a lot of work on the raw item scriptable object, so it will need special attention
        // IL.HUDManager.CreateToolAdModel += DynamicallyReplaceAudioClips; - works on raw scriptable object

        IL.GrabbableObject.FallToGround += DynamicallyReplaceItemProperties;
        IL.GrabbableObject.GetItemFloorPosition += DynamicallyReplaceItemProperties;
        IL.GrabbableObject.GetPhysicsRegionOfDroppedObject += DynamicallyReplaceItemProperties;
        IL.GrabbableObject.FallWithCurve += DynamicallyReplaceItemProperties;
        IL.GrabbableObject.Update += DynamicallyReplaceItemProperties;
        IL.GrabbableObject.LateUpdate += DynamicallyReplaceItemProperties;

        IL.CaveDwellerPhysicsProp.Update += DynamicallyReplaceItemProperties;
        IL.CaveDwellerPhysicsProp.LateUpdate += DynamicallyReplaceItemProperties;

        IL.SoccerBallProp.FallWithCurve += DynamicallyReplaceItemProperties;
        IL.StunGrenadeItem.FallWithCurve += DynamicallyReplaceItemProperties;

        DuskPlugin.Logger.LogInfo("Done 'DynamicallyReplaceAudioClips' patching!");
    }

    private static void RegisterDuskUnlockables(On.Terminal.orig_Awake orig, Terminal self)
    {
        if (LethalContent.Unlockables.IsFrozen)
        {
            orig(self);
            return;
        }

        foreach (DawnUnlockableItemInfo unlockableItemInfo in LethalContent.Unlockables.Values)
        {
            if (unlockableItemInfo.UnlockableItem.prefabObject == null)
                continue;

            if (unlockableItemInfo.UnlockableItem.prefabObject.GetComponent<DuskUnlockable>())
                continue;

            unlockableItemInfo.UnlockableItem.prefabObject.AddComponent<DuskUnlockable>();
        }

        foreach (UnlockableItem unlockableItem in StartOfRoundRefs.Instance.unlockablesList.unlockables)
        {
            if (unlockableItem.prefabObject == null)
                continue;

            if (unlockableItem.prefabObject.GetComponent<DuskUnlockable>())
                continue;

            unlockableItem.prefabObject.AddComponent<DuskUnlockable>();
        }
        orig(self);
    }

    // note!!! this transpiler should only be used on enemy AIs!
    private static readonly Dictionary<string, Func<GrabbableObject, Vector3, Vector3>> offsetReplacerFunctions = new()
    {
        { nameof(Item.restingRotation), GenerateOffsetReplacer(it => it.RestingRotation) },
        { nameof(Item.rotationOffset), GenerateOffsetReplacer(it => it.RotationOffset) },
        { nameof(Item.positionOffset), GenerateOffsetReplacer(it => it.PositionOffset) }
    };

    private static void DynamicallyReplaceItemProperties(ILContext il)
    {
        Debuggers.Patching?.Log($"patching: {il.Method.Name} with {nameof(DynamicallyReplaceItemProperties)}. il count {il.Body.Instructions.Count}");
        ILCursor c = new ILCursor(il);

        // evil for loop.
        for (; c.Index < c.Instrs.Count; c.Index++)
        {
            if (c.Next.OpCode != OpCodes.Ldfld)
                continue;

            if (c.Next.MatchLdfld<Item>(nameof(Item.verticalOffset)))
            {
                c.Index--;
                c.Emit(OpCodes.Dup);

                c.Index += 2;
                c.EmitDelegate<Func<GrabbableObject, float, float>>((self, existing) =>
                {
                    if (!self.TryGetGrabbableObjectReplacement(out var replacement))
                    {
                        return existing;
                    }
                    return replacement.VerticalOffset;
                });
                continue;
            }

            if (c.Next.MatchLdfld<Item>(nameof(Item.floorYOffset)))
            {
                c.Index--;
                c.Emit(OpCodes.Dup);

                c.Index += 2;
                c.EmitDelegate<Func<GrabbableObject, int, int>>((self, existing) =>
                {
                    if (!self.TryGetGrabbableObjectReplacement(out var replacement))
                    {
                        return existing;
                    }
                    return replacement.FloorYOffset;
                });
                continue;
            }

            foreach ((string name, var replacer) in offsetReplacerFunctions)
            {
                if (!c.Next.MatchLdfld<Item>(name))
                    continue;

                c.Index--;
                c.Emit(OpCodes.Dup);

                c.Index += 2;
                c.EmitDelegate(replacer);
                break;
            }
        }
    }

    private static void RegisterMapObjectReplacements()
    {
        foreach (DuskEntityReplacementDefinition entityReplacementDefinition in DuskModContent.EntityReplacements.Values)
        {
            if (entityReplacementDefinition is not DuskMapObjectReplacementDefinition mapObjectReplacementDefinition)
                continue;

            if (LethalContent.MapObjects.TryGetValue(mapObjectReplacementDefinition.EntityToReplaceKey, out DawnMapObjectInfo mapObjectInfo))
            {
                if (!mapObjectInfo.CustomData.TryGet(Key, out List<DuskMapObjectReplacementDefinition>? list))
                {
                    DuskMapObjectReplacementDefinition vanilla = ScriptableObject.CreateInstance<DuskMapObjectReplacementDefinition>();
                    vanilla.IsDefault = true;
                    vanilla.Register(null);
                    list = [vanilla];
                    mapObjectInfo.CustomData.Set(Key, list);
                }
                list.Add(mapObjectReplacementDefinition);
            }
        }

        foreach (DawnMapObjectInfo mapObjectInfo in LethalContent.MapObjects.Values)
        {
            if (mapObjectInfo.MapObject.GetComponent<DuskMapObject>())
                continue;

            mapObjectInfo.MapObject.AddComponent<DuskMapObject>();
        }
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

        if (self.TryGetGrabbableObjectReplacement(out var _))
        {
            orig(self);
            return;
        }

        if (!self.itemProperties.GetDawnInfo().CustomData.TryGet(Key, out List<DuskItemReplacementDefinition>? replacements))
        {
            orig(self);
            return;
        }

        List<DuskItemReplacementDefinition> newReplacements = new List<DuskItemReplacementDefinition>(replacements);
        for (int i = newReplacements.Count - 1; i >= 0; i--)
        {
            DuskItemReplacementDefinition replacement = newReplacements[i];
            if (replacement.DatePredicate == null)
                continue;

            if (!replacement.DatePredicate.Evaluate())
            {
                newReplacements.RemoveAt(i);
            }
        }

        // todo: save the current skin and try to restore it if this runs in orbit
        /*if (StartOfRound.Instance.inShipPhase)
        {
            orig(self);
            return;
        }*/

        DawnMoonInfo currentMoon = RoundManager.Instance.currentLevel.GetDawnInfo();

        SpawnWeightContext ctx = new(currentMoon, RoundManager.Instance.dungeonGenerator?.Generator?.DungeonFlow?.GetDawnInfo(), TimeOfDayRefs.GetCurrentWeatherEffect(currentMoon.Level)?.GetDawnInfo());
        int? totalWeight = newReplacements.Sum(it => it.Weights.GetFor(currentMoon, ctx));
        if (totalWeight == null)
        {
            orig(self);
            return;
        }

        replacementRandom ??= new Random(StartOfRound.Instance.randomMapSeed + 234780);

        int chosenWeight = replacementRandom.Next(0, totalWeight.Value);
        foreach (DuskItemReplacementDefinition replacement in newReplacements)
        {
            chosenWeight -= replacement.Weights.GetFor(currentMoon, ctx) ?? 0;
            if (chosenWeight > 0)
                continue;

            if (replacement.IsDefault)
                break;

            StartOfRoundRefs.Instance.StartCoroutine(replacement.Apply(self));
            break;
        }
        orig(self);
    }

    private static void ReplaceEnemyEntityUsingNest(On.EnemyAI.orig_UseNestSpawnObject orig, EnemyAI self, EnemyAINestSpawnObject nestSpawnObject)
    {
        if (nestSpawnObject.HasNestReplacement())
        {
            DuskEnemyReplacementDefinition enemyReplacementDefinition = nestSpawnObject.GetNestReplacement()!;
            StartOfRoundRefs.Instance.StartCoroutine(enemyReplacementDefinition.Apply(self));
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
                    if (!self.TryGetEnemyReplacement(out var replacement))
                    {
                        return existingAudioClips;
                    }
                    return replacement.AudioClips;
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

    static Func<GrabbableObject, Vector3, Vector3> GenerateOffsetReplacer(Func<DuskItemReplacementDefinition, Vector3> generator)
    {
        return (self, existing) =>
        {
            ICurrentEntityReplacement replacement = (ICurrentEntityReplacement)self;
            if (replacement.CurrentEntityReplacement == null)
                return existing;

            return generator((DuskItemReplacementDefinition)replacement.CurrentEntityReplacement);
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
        SpawnWeightContext ctx = new(currentMoon, RoundManager.Instance.dungeonGenerator?.Generator?.DungeonFlow?.GetDawnInfo(), TimeOfDayRefs.GetCurrentWeatherEffect(currentMoon.Level)?.GetDawnInfo());
        int? totalWeight = replacements.Sum(it => it.Weights.GetFor(currentMoon, ctx));
        if (totalWeight == null)
        {
            return;
        }

        replacementRandom ??= new Random(StartOfRound.Instance.randomMapSeed + 234780);

        int chosenWeight = replacementRandom.Next(0, totalWeight.Value);
        foreach (DuskEnemyReplacementDefinition replacement in replacements)
        {
            chosenWeight -= replacement.Weights.GetFor(currentMoon, ctx) ?? 0;
            if (chosenWeight > 0)
                continue;

            if (replacement.IsDefault)
                break;

            StartOfRoundRefs.Instance.StartCoroutine(replacement.ApplyNest(self));
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
                if (!enemyInfo.CustomData.TryGet(Key, out List<DuskEnemyReplacementDefinition>? list))
                {
                    DuskEnemyReplacementDefinition defaultSkin = ScriptableObject.CreateInstance<DuskEnemyReplacementDefinition>();
                    defaultSkin.IsDefault = true;
                    defaultSkin.Register(null);
                    list = [defaultSkin];
                    enemyInfo.CustomData.Set(Key, list);
                }
                list.Add(enemyReplacementDefinition);
            }
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

        if (self.TryGetEnemyReplacement(out var _))
        {
            return;
        }

        List<DuskEnemyReplacementDefinition> newReplacements = new List<DuskEnemyReplacementDefinition>(replacements);
        for (int i = newReplacements.Count - 1; i >= 0; i--)
        {
            DuskEnemyReplacementDefinition replacement = newReplacements[i];
            if (replacement.DatePredicate == null)
                continue;

            if (!replacement.DatePredicate.Evaluate())
            {
                newReplacements.RemoveAt(i);
            }
        }

        DawnMoonInfo currentMoon = RoundManager.Instance.currentLevel.GetDawnInfo();
        SpawnWeightContext ctx = new(currentMoon, RoundManager.Instance.dungeonGenerator?.Generator?.DungeonFlow?.GetDawnInfo(), TimeOfDayRefs.GetCurrentWeatherEffect(currentMoon.Level)?.GetDawnInfo());
        int? totalWeight = newReplacements.Sum(it => it.Weights.GetFor(currentMoon, ctx));
        if (totalWeight == null)
        {
            return;
        }

        replacementRandom ??= new Random(StartOfRound.Instance.randomMapSeed + 234780);

        int chosenWeight = replacementRandom.Next(0, totalWeight.Value);
        foreach (DuskEnemyReplacementDefinition replacement in newReplacements)
        {
            chosenWeight -= replacement.Weights.GetFor(currentMoon, ctx) ?? 0;
            if (chosenWeight > 0)
                continue;

            if (replacement.IsDefault)
                break;

            StartOfRoundRefs.Instance.StartCoroutine(replacement.Apply(self));
            break;
        }
    }
}