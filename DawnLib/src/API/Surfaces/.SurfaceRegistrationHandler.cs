using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Dawn.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace Dawn;

[HarmonyPatch]
static class SurfaceRegistrationHandler
{
    internal static void Init()
    {
        using (new DetourContext(priority: int.MaxValue))
        {
            On.StartOfRound.Awake += RegisterDawnSurfaces;
        }
        IL.GameNetcodeStuff.PlayerControllerB.Update += EditGravityDirection;
        IL.GameNetcodeStuff.PlayerControllerB.Update += EditGravityStrength;
    }

    private static void EditGravityStrength(ILContext il)
    {
        ILCursor cursor = new(il);
        if (!cursor.TryGotoNext(
            MoveType.After,
            instr => instr.MatchLdarg(0),
            instr => instr.MatchLdarg(0),
            instr => instr.MatchLdfld<PlayerControllerB>("fallValue"),
            instr => instr.MatchLdcR4(38),
            instr => instr.MatchCall(typeof(Time), "get_deltaTime"),
            instr => instr.MatchMul()
            ))
        {
            DawnPlugin.Logger.LogError($"Couldn't match GameNetcodeStuff.PlayerControllerB.Update IL for gravity strength control.");
            return;
        }

        cursor.Emit(Mono.Cecil.Cil.OpCodes.Call, AccessTools.Method(typeof(SurfaceRegistrationHandler), nameof(GetGravityStrength)));
        cursor.Emit(Mono.Cecil.Cil.OpCodes.Mul);
    }

    private static float GetGravityStrength()
    {
        return 1f;
    }

    private static void EditGravityDirection(ILContext il)
    {
        ILCursor cursor = new(il);
        if (!cursor.TryGotoNext(
            MoveType.Before,
            instr => instr.MatchLdcR4(0),
            instr => instr.MatchLdarg(0),
            instr => instr.MatchLdfld<PlayerControllerB>("fallValue"),
            instr => instr.MatchLdcR4(0),
            instr => instr.MatchNewobj(typeof(Vector3))
            ))
        {
            DawnPlugin.Logger.LogError($"Couldn't match GameNetcodeStuff.PlayerControllerB.Update IL for gravity direction control.");
            return;
        }

        cursor.RemoveRange(5);

        MethodInfo multiplyVector3 = AccessTools.Method(typeof(Vector3), "op_Multiply", [typeof(Vector3), typeof(float)]);

        cursor.Emit(Mono.Cecil.Cil.OpCodes.Call, AccessTools.Method(typeof(SurfaceRegistrationHandler), nameof(GetGravityDirection)));

        cursor.Emit(Mono.Cecil.Cil.OpCodes.Ldc_R4, -1f);
        cursor.Emit(Mono.Cecil.Cil.OpCodes.Call, multiplyVector3);

        cursor.Emit(Mono.Cecil.Cil.OpCodes.Ldarg_0);
        cursor.Emit(Mono.Cecil.Cil.OpCodes.Ldfld, AccessTools.Field(typeof(PlayerControllerB), "fallValue"));

        cursor.Emit(Mono.Cecil.Cil.OpCodes.Call, multiplyVector3);
    }

    private static Vector3 GetGravityDirection()
    {
        Vector3 direction = Vector3.down;
        return direction.normalized;
    }

    private static void RegisterDawnSurfaces(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        CollectVanillaSurfaces(self);
        List<FootstepSurface?> newSurfaces = [.. self.footstepSurfaces];
        foreach (DawnSurfaceInfo surfaceInfo in LethalContent.Surfaces.Values)
        {
            if (surfaceInfo.ShouldSkipIgnoreOverride())
                continue;

            surfaceInfo.SurfaceIndex = newSurfaces.Count;
            newSurfaces.Add(surfaceInfo.Surface);
        }
        self.footstepSurfaces = newSurfaces.ToArray();
        orig(self);
    }

    private static void CollectVanillaSurfaces(StartOfRound startOfRound)
    {
        if (LethalContent.Surfaces.IsFrozen)
        {
            return;
        }

        for (int i = 0; i < startOfRound.footstepSurfaces.Length; i++)
        {
            FootstepSurface? surface = startOfRound.footstepSurfaces[i];

            if (surface == null || surface.HasDawnInfo())
                continue;

            string surfaceTag = NamespacedKey.NormalizeStringForNamespacedKey(surface.surfaceTag, true);
            NamespacedKey<DawnSurfaceInfo>? key = SurfaceKeys.GetByReflection(surfaceTag);

            if (key == null)
            {
                key = NamespacedKey<DawnSurfaceInfo>.From("unknown_lib", surface.surfaceTag);
            }

            if (LethalContent.Surfaces.ContainsKey(key))
            {
                DawnPlugin.Logger.LogWarning($"Surface {surface.surfaceTag} is already registered by the same creator to LethalContent. This is likely to cause issues unless caused by lobby reloads.");
                LethalContent.Surfaces[key].Surface = surface;
                surface.SetDawnInfo(LethalContent.Surfaces[key]);
                continue;
            }

            DawnSurfaceInfo surfaceInfo = new(key, [DawnLibTags.IsExternal], surface, null, Vector3.zero, i, null);
            LethalContent.Surfaces.Register(surfaceInfo);
            surface.SetDawnInfo(surfaceInfo);
        }
        LethalContent.Surfaces.Freeze();
    }

    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.GetCurrentMaterialStandingOn)), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> GetCurrentMaterialStandingOn(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        return new CodeMatcher(instructions, generator).MatchForward(useEnd: false,
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldflda, AccessTools.Field(typeof(PlayerControllerB), nameof(PlayerControllerB.hit))),
                new(OpCodes.Call, AccessTools.Method(typeof(RaycastHit), "get_collider")),
                new(OpCodes.Call, AccessTools.Method(typeof(StartOfRound), "get_Instance")),
                new(OpCodes.Ldfld, AccessTools.Field(typeof(StartOfRound), nameof(StartOfRound.footstepSurfaces))))
            .CreateLabel(out Label vanillaFootstep)
            .Insert(
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldflda, AccessTools.Field(typeof(PlayerControllerB), nameof(PlayerControllerB.hit))),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldflda, AccessTools.Field(typeof(PlayerControllerB), nameof(PlayerControllerB.currentFootstepSurfaceIndex))),
                new(OpCodes.Call, AccessTools.Method(typeof(SurfaceRegistrationHandler), nameof(TryGetAndSetDawnSurfaceIndex))),
                new(OpCodes.Brfalse_S, vanillaFootstep),
                new(OpCodes.Ret))
            .InstructionEnumeration();
    }

    [HarmonyPatch(typeof(MaskedPlayerEnemy), nameof(MaskedPlayerEnemy.GetMaterialStandingOn)), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> GetMaterialStandingOn(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        return new CodeMatcher(instructions, generator).MatchForward(useEnd: false,
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldflda, AccessTools.Field(typeof(MaskedPlayerEnemy), nameof(MaskedPlayerEnemy.enemyRayHit))),
                new(OpCodes.Call, AccessTools.Method(typeof(RaycastHit), "get_collider")),
                new(OpCodes.Call, AccessTools.Method(typeof(StartOfRound), "get_Instance")),
                new(OpCodes.Ldfld, AccessTools.Field(typeof(StartOfRound), nameof(StartOfRound.footstepSurfaces))))
            .CreateLabel(out Label vanillaFootstep)
            .Insert(
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldflda, AccessTools.Field(typeof(MaskedPlayerEnemy), nameof(MaskedPlayerEnemy.enemyRayHit))),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldflda, AccessTools.Field(typeof(MaskedPlayerEnemy), nameof(MaskedPlayerEnemy.currentFootstepSurfaceIndex))),
                new(OpCodes.Call, AccessTools.Method(typeof(SurfaceRegistrationHandler), nameof(TryGetAndSetDawnSurfaceIndex))),
                new(OpCodes.Brfalse_S, vanillaFootstep),
                new(OpCodes.Ret))
            .InstructionEnumeration();
    }

    private static bool TryGetAndSetDawnSurfaceIndex(ref RaycastHit hit, ref int currentFootstepSurfaceIndex)
    {
        if (hit.collider.TryGetComponent(out DawnSurface surface) && surface.SurfaceIndex > 0)
        {
            currentFootstepSurfaceIndex = surface.SurfaceIndex;
            DawnSurfaceInfo surfaceInfo = StartOfRound.Instance.footstepSurfaces[currentFootstepSurfaceIndex].GetDawnInfo();
            if (surfaceInfo.SurfaceVFXPrefab != null)
            {
                FootstepVFXPool.Instance!.Play(surfaceInfo.SurfaceVFXPrefab, hit.point, hit.normal, surfaceInfo.SurfaceVFXOffset, 1f);
            }
            return true; // DawnSurface found, return early!
        }

        return false; // Vanilla surface, do nothin'.
    }

    [HarmonyPatch(typeof(Shovel), nameof(Shovel.HitShovel)), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> HitShovel(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        CodeMatcher codeMatcher = new CodeMatcher(instructions, generator).End()
            .MatchBack(useEnd: true,
                new(OpCodes.Ldc_I4_1),
                new(OpCodes.Stloc_0),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld, AccessTools.Field(typeof(Shovel), nameof(Shovel.objectsHitByShovelList))),
                new(OpCodes.Ldloc_S));

        object objectHitByShovelIndex = codeMatcher.Operand; // Current collider index (`V_7`).
        int position = codeMatcher.Pos - 2; // Start of the block to insert instructions before.

        object continueLocation = codeMatcher.MatchForward(useEnd: true, // Instruction to jump to if a DawnSurface is found (acts as `continue`).
            new(OpCodes.Stloc_3),
            new(OpCodes.Br)).Operand;

        return codeMatcher.Advance(position - codeMatcher.Pos) // Return to prior position.
            .CreateLabel(out Label vanillaFootstep)
            .Insert(
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld, AccessTools.Field(typeof(Shovel), nameof(Shovel.objectsHitByShovelList))),
                new(OpCodes.Ldloc_S, objectHitByShovelIndex),
                new(OpCodes.Call, AccessTools.Method(typeof(SurfaceRegistrationHandler), nameof(GetDawnSurfaceIndex))),
                new(OpCodes.Stloc_3),
                new(OpCodes.Ldloc_3),
                new(OpCodes.Ldc_I4_M1),
                new(OpCodes.Beq_S, vanillaFootstep), // Continue as a vanilla footstep if current index is `-1`.
                new(OpCodes.Br_S, continueLocation)) // Skip to next loop cycle if a DawnSurface was found.
            .InstructionEnumeration();
    }

    [HarmonyPatch(typeof(KnifeItem), nameof(KnifeItem.HitKnife)), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> HitKnife(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        CodeMatcher codeMatcher = new CodeMatcher(instructions, generator).End()
            .MatchBack(useEnd: true,
                new(OpCodes.Ldc_I4_1),
                new(OpCodes.Stloc_0),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld, AccessTools.Field(typeof(KnifeItem), nameof(KnifeItem.objectsHitByKnifeList))),
                new(OpCodes.Ldloc_S));

        object objectHitByKnifeIndex = codeMatcher.Operand; // Current collider index (`V_7`).
        int position = codeMatcher.Pos - 2; // Start of the block to insert instructions before.

        object continueLocation = codeMatcher.MatchForward(useEnd: true, // Instruction to jump to if a DawnSurface is found (acts as `continue`).
            new(OpCodes.Stloc_2),
            new(OpCodes.Br)).Operand;

        return codeMatcher.Advance(position - codeMatcher.Pos) // Return to prior position.
            .CreateLabel(out Label vanillaFootstep)
            .Insert(
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld, AccessTools.Field(typeof(KnifeItem), nameof(KnifeItem.objectsHitByKnifeList))),
                new(OpCodes.Ldloc_S, objectHitByKnifeIndex),
                new(OpCodes.Call, AccessTools.Method(typeof(SurfaceRegistrationHandler), nameof(GetDawnSurfaceIndex))),
                new(OpCodes.Stloc_2),
                new(OpCodes.Ldloc_2),
                new(OpCodes.Ldc_I4_M1),
                new(OpCodes.Beq_S, vanillaFootstep), // Continue as a vanilla footstep if current index is `-1`.
                new(OpCodes.Br_S, continueLocation)) // Skip to next loop cycle if a DawnSurface was found.
            .InstructionEnumeration();
    }

    private static int GetDawnSurfaceIndex(List<RaycastHit> objectsHitList, int objectHitIndex)
    {
        Collider? surfaceCollider = objectsHitList[objectHitIndex].collider;

        if (surfaceCollider != null && surfaceCollider.TryGetComponent(out DawnSurface surface))
        {
            return surface.SurfaceIndex; // DawnSurface found, return surface index!
        }

        return -1; // Vanilla surface, return no index.
    }
}