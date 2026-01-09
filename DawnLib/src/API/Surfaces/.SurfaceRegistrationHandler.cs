using System.Collections.Generic;
using System.Reflection.Emit;
using Dawn.Internal;
using Dawn.Utils;
using GameNetcodeStuff;
using HarmonyLib;
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
                key = NamespacedKey<DawnSurfaceInfo>.From("unknown_lib", NamespacedKey.NormalizeStringForNamespacedKey(surface.surfaceTag, false));
            }

            if (LethalContent.Surfaces.ContainsKey(key))
            {
                DawnPlugin.Logger.LogWarning($"Surface {surface.surfaceTag} is already registered by the same creator to LethalContent. This is likely to cause issues unless caused by lobby reloads.");
                LethalContent.Surfaces[key].Surface = surface;
                surface.SetDawnInfo(LethalContent.Surfaces[key]);
                continue;
            }

            DawnSurfaceInfo surfaceInfo = new(key, [DawnLibTags.IsExternal], surface, i, null);
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
                new(OpCodes.Call, AccessTools.Method(typeof(RaycastHit), "get_collider")),
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
                new(OpCodes.Call, AccessTools.Method(typeof(RaycastHit), "get_collider")),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldflda, AccessTools.Field(typeof(MaskedPlayerEnemy), nameof(MaskedPlayerEnemy.currentFootstepSurfaceIndex))),
                new(OpCodes.Call, AccessTools.Method(typeof(SurfaceRegistrationHandler), nameof(TryGetAndSetDawnSurfaceIndex))),
                new(OpCodes.Brfalse_S, vanillaFootstep),
                new(OpCodes.Ret))
            .InstructionEnumeration();
    }

    /* [HarmonyPatch(typeof(Shovel), nameof(Shovel.HitShovel)), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> HitShovel(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        CodeMatcher codeMatcher = new CodeMatcher(instructions, generator)
            .MatchForward(useEnd: false,
                new(OpCodes.Ldc_I4_1),
                new(OpCodes.Ldc_I4_1),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld, AccessTools.Field(typeof(Shovel), nameof(Shovel.objectsHitByShovelList))))
            .Advance(2)
            .CreateLabel(out Label vanillaSurface)
            .Insert(
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld, AccessTools.Field(typeof(Shovel), nameof(Shovel.objectsHitByShovelList))),
                new(OpCodes.Call, AccessTools.Method(typeof(RaycastHit), "get_collider")),
                new(OpCodes.Ldloc_S));
        return codeMatcher.InstructionEnumeration();
    } */

    private static bool TryGetAndSetDawnSurfaceIndex(Collider collider, ref int currentFootstepSurfaceIndex)
    {
        if (collider.TryGetComponent(out DawnSurface surface) && surface.SurfaceIndex > 0)
        {
            currentFootstepSurfaceIndex = surface.SurfaceIndex;

            return true; // DawnSurface found, return early!
        }

        return false; // Vanilla surface, do nothin'.
    }
}