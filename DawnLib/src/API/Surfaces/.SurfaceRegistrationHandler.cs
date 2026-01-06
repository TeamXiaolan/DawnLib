using System.Collections.Generic;
using System.Reflection.Emit;
using Dawn.Internal;
using Dawn.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace Dawn;

[HarmonyPatch]
static class SurfaceRegistrationHandler
{
    internal static void Init()
    {
        On.StartOfRound.Awake += CollectVanillaSurfaces;
        On.StartOfRound.Start += RegisterDawnSurfaces;
    }

    private static void RegisterDawnSurfaces(On.StartOfRound.orig_Start orig, StartOfRound self)
    {
        orig(self);
        if (LethalContent.Surfaces.IsFrozen)
        {
            return;
        }

        List<FootstepSurface?> newSurfaces = [.. StartOfRoundRefs.Instance.footstepSurfaces];
        foreach (DawnSurfaceInfo surfaceInfo in LethalContent.Surfaces.Values)
        {
            if (surfaceInfo.ShouldSkipIgnoreOverride())
                continue;

            newSurfaces.Add(surfaceInfo.Surface);
            surfaceInfo.SurfaceIndex = newSurfaces.Count;
        }
        StartOfRoundRefs.Instance.footstepSurfaces = newSurfaces.ToArray();
        LethalContent.StoryLogs.Freeze();
    }

    private static void CollectVanillaSurfaces(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        orig(self);
        if (LethalContent.Surfaces.IsFrozen)
        {
            return;
        }

        for (int i = 0; i < StartOfRoundRefs.Instance.footstepSurfaces.Length; i++)
        {
            FootstepSurface? surface = StartOfRoundRefs.Instance.footstepSurfaces[i];

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
    }

    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.GetCurrentMaterialStandingOn)), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> GetCurrentMaterialStandingOn(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        return new CodeMatcher(instructions, generator).MatchForward(useEnd: false,
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldloc_0),
                new(OpCodes.Stfld, AccessTools.Field(typeof(PlayerControllerB), nameof(PlayerControllerB.currentFootstepSurfaceIndex))))
            .Advance(-1)
            .MatchBack(useEnd: false, new CodeMatch(OpCodes.Ldarg_0))
            .CreateLabel(out Label vanillaFootstep)
            .InsertAndAdvance(
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldflda, AccessTools.Field(typeof(PlayerControllerB), nameof(PlayerControllerB.hit))),
                new(OpCodes.Call, AccessTools.Method(typeof(RaycastHit), "get_collider")),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldflda, AccessTools.Field(typeof(PlayerControllerB), nameof(PlayerControllerB.currentFootstepSurfaceIndex))),
                new(OpCodes.Call, AccessTools.Method(typeof(SurfaceRegistrationHandler), nameof(TryGetAndSetDawnSurfaceIndex))),
                new(OpCodes.Brfalse, vanillaFootstep),
                new(OpCodes.Ret))
            .InstructionEnumeration();
    }

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