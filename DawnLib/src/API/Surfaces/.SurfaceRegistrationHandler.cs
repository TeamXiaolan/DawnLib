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
using UnityEngine.SceneManagement;

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

        IL.GameNetcodeStuff.PlayerControllerB.GetCurrentMaterialStandingOn += PlayerGetCurrentMaterialStandingOn;
        IL.GameNetcodeStuff.PlayerControllerB.Update += EditGravityDirection;
        IL.GameNetcodeStuff.PlayerControllerB.Update += EditFallValueForGravity;
        IL.GameNetcodeStuff.PlayerControllerB.Update += EditUncappedFallValueForGravity;

        SceneManager.sceneLoaded += OnVowOrMarchLoaded;
        SceneManager.sceneLoaded += TryFixMoonTerrainFootsteps;
    }

    private static void TryFixMoonTerrainFootsteps(Scene arg0, LoadSceneMode arg1)
    {
        if (StartOfRound.Instance == null)
        {
            return;
        }

        if (RoundManager.Instance.currentLevel == null)
        {
            return;
        }

        DawnMoonInfo moonInfo = RoundManager.Instance.currentLevel.GetDawnInfo();
        if (moonInfo == null || moonInfo.TypedKey.IsVanilla())
        {
            return;
        }

        TerrainCollider terrainCollider = GameObject.FindAnyObjectByType<TerrainCollider>();
        TerrainData terrainData = terrainCollider.terrainData;
        int textureLayerCount = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight).Length / (terrainData.alphamapWidth * terrainData.alphamapHeight);
        if (terrainCollider == null || terrainCollider.terrainData == null || textureLayerCount == 0 || !terrainCollider.gameObject.activeSelf || !terrainCollider.enabled || !terrainCollider.gameObject.activeInHierarchy)
        {
            return;
        }

        if (terrainCollider.gameObject.CompareTag("Untagged"))
        {
            return;
        }

        if (terrainCollider.gameObject.GetComponent<DawnSurface>() != null)
        {
            return;
        }

        DawnSurface surface = terrainCollider.gameObject.AddComponent<DawnSurface>();
        foreach (DawnSurfaceInfo surfaceInfo in LethalContent.Surfaces.Values)
        {
            if (!surfaceInfo.Surface.surfaceTag.Equals(terrainCollider.gameObject.tag, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            for (int i = 0; i < textureLayerCount; i++)
            {
                surface.NamespacedKeysForTerrain.Add(surfaceInfo.Key);
            }
        }
    }

    private static void OnVowOrMarchLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (StartOfRound.Instance == null)
        {
            return;
        }

        SelectableLevel level = RoundManager.Instance.currentLevel;
        if (level == LethalContent.Moons[MoonKeys.March].Level)
        {
            GameObject marchTerrain = GameObject.FindGameObjectWithTag("OutsideLevelNavMesh").transform.Find("Map/MarchTerrainNew").gameObject;
            DawnSurface surface = marchTerrain.AddComponent<DawnSurface>();
            surface.NamespacedKeysForTerrain.AddRange([NamespacedKey.From("lethal_company", "grass"), NamespacedKey.From("lethal_company", "rock"), NamespacedKey.From("lethal_company", "gravel")]);
        }
        else if (level == LethalContent.Moons[MoonKeys.Vow].Level)
        {
            GameObject vowTerrain = GameObject.FindGameObjectWithTag("OutsideLevelNavMesh").transform.Find("VowTerrain").gameObject;
            DawnSurface surface = vowTerrain.AddComponent<DawnSurface>();
            surface.NamespacedKeysForTerrain.AddRange([NamespacedKey.From("lethal_company", "grass"), NamespacedKey.From("lethal_company", "gravel"), NamespacedKey.From("lethal_company", "rock")]);
        }
    }

    private static void PlayerGetCurrentMaterialStandingOn(ILContext il)
    {
        ILCursor cursor = new(il);

        if (!cursor.TryGotoNext(
            MoveType.After,
            instr => instr.MatchCall(AccessTools.Method(typeof(StartOfRound), "get_Instance")),
            instr => instr.MatchLdfld(AccessTools.Field(typeof(StartOfRound), "walkableSurfacesMask")),
            instr => instr.MatchLdcI4(1),
            instr => instr.MatchCall(AccessTools.Method(typeof(Physics), "Raycast", [typeof(Ray), typeof(RaycastHit).MakeByRefType(), typeof(float), typeof(int), typeof(QueryTriggerInteraction)])),
            instr => instr.MatchBrfalse(out _)
            ))
        {
            DawnPlugin.Logger.LogError($"Couldn't match GameNetcodeStuff.PlayerControllerB.GetCurrentMaterialStandingOn IL.");
            return;
        }

        // This label should point to the FIRST ORIGINAL instruction of the vanilla body.
        ILLabel runVanillaLogic = cursor.DefineLabel();

        cursor.Emit(Mono.Cecil.Cil.OpCodes.Ldarg_0);
        cursor.Emit(Mono.Cecil.Cil.OpCodes.Ldarg_1);
        cursor.Emit(Mono.Cecil.Cil.OpCodes.Ldarg_0);
        cursor.EmitLdflda<PlayerControllerB>(nameof(PlayerControllerB.hit));
        cursor.Emit(Mono.Cecil.Cil.OpCodes.Ldarg_0);
        cursor.EmitLdflda<PlayerControllerB>(nameof(PlayerControllerB.currentFootstepSurfaceIndex));
        cursor.Emit(Mono.Cecil.Cil.OpCodes.Call, AccessTools.Method(typeof(SurfaceRegistrationHandler), nameof(TryGetAndSetDawnSurfaceIndexPlayer)));
        // false => continue into original vanilla code
        cursor.Emit(Mono.Cecil.Cil.OpCodes.Brfalse, runVanillaLogic);

        // true => we handled it, leave method
        cursor.Emit(Mono.Cecil.Cil.OpCodes.Ret);

        // Mark the next ORIGINAL instruction as the vanilla fallback entry point
        cursor.MarkLabel(runVanillaLogic);
    }

    private static void EditUncappedFallValueForGravity(ILContext il)
    {
        ILCursor cursor = new(il);
        if (!cursor.TryGotoNext(
            MoveType.After,
            instr => instr.MatchLdarg(0),
            instr => instr.MatchLdarg(0),
            instr => instr.MatchLdfld<PlayerControllerB>("fallValueUncapped"),
            instr => instr.MatchLdcR4(26),
            instr => instr.MatchCall<Time>("get_deltaTime"),
            instr => instr.MatchMul()
        ))
        {
            DawnPlugin.Logger.LogError($"Couldn't match GameNetcodeStuff.PlayerControllerB.Update IL for normal gravity strength control 1.");
            return;
        }

        cursor.Emit(Mono.Cecil.Cil.OpCodes.Call, AccessTools.Method(typeof(SurfaceRegistrationHandler), nameof(GetGravityStrength)));
        cursor.Emit(Mono.Cecil.Cil.OpCodes.Mul);

        if (!cursor.TryGotoNext(
            MoveType.After,
            instr => instr.MatchLdarg(0),
            instr => instr.MatchLdarg(0),
            instr => instr.MatchLdfld<PlayerControllerB>("fallValueUncapped"),
            instr => instr.MatchLdcR4(38),
            instr => instr.MatchCall<Time>("get_deltaTime"),
            instr => instr.MatchMul()
            ))
        {
            DawnPlugin.Logger.LogError($"Couldn't match GameNetcodeStuff.PlayerControllerB.Update IL for uncapped gravity strength control 2.");
            return;
        }

        cursor.Emit(Mono.Cecil.Cil.OpCodes.Call, AccessTools.Method(typeof(SurfaceRegistrationHandler), nameof(GetGravityStrength)));
        cursor.Emit(Mono.Cecil.Cil.OpCodes.Mul);
    }

    private static void EditFallValueForGravity(ILContext il)
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
            DawnPlugin.Logger.LogError($"Couldn't match GameNetcodeStuff.PlayerControllerB.Update IL for normal gravity strength control.");
            return;
        }

        cursor.Emit(Mono.Cecil.Cil.OpCodes.Call, AccessTools.Method(typeof(SurfaceRegistrationHandler), nameof(GetGravityStrength)));
        cursor.Emit(Mono.Cecil.Cil.OpCodes.Mul);
    }

    private static float GetGravityStrength()
    {
        float gravityStrength = 1f;
        if (GameNetworkManager.Instance.localPlayerController.TryGetCurrentDawnSurface(out DawnSurface? dawnSurface))
        {
            gravityStrength = dawnSurface.GravityStrength;
        }
        return gravityStrength;
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
        if (!GameNetworkManager.Instance.localPlayerController.TryGetCurrentDawnSurface(out DawnSurface? dawnSurface) || dawnSurface.GravityCenter == null)
        {
            return Vector3.down;
        }

        Vector3 gravityDirection = dawnSurface.GravityCenter.transform.position - GameNetworkManager.Instance.localPlayerController.transform.position;
        return gravityDirection.normalized;
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

        if (LethalContent.Surfaces.IsFrozen)
        {
            return;
        }
        LethalContent.Surfaces.Freeze();
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
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldflda, AccessTools.Field(typeof(MaskedPlayerEnemy), nameof(MaskedPlayerEnemy.enemyRayHit))),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldflda, AccessTools.Field(typeof(MaskedPlayerEnemy), nameof(MaskedPlayerEnemy.currentFootstepSurfaceIndex))),
                new(OpCodes.Call, AccessTools.Method(typeof(SurfaceRegistrationHandler), nameof(TryGetAndSetDawnSurfaceIndexMasked))),
                new(OpCodes.Brfalse_S, vanillaFootstep),
                new(OpCodes.Ret))
            .InstructionEnumeration();
    }

    private static bool TryGetAndSetDawnSurfaceIndexPlayer(PlayerControllerB playerControllerB, bool checkStandingOnTerrain, ref RaycastHit hit, ref int currentFootstepSurfaceIndex)
    {
        if (hit.collider.TryGetComponent(out DawnSurface surface) && (surface.SurfaceIndex > -1 || surface.TerrainIndices.Count > 0))
        {
            playerControllerB.SetCurrentDawnSurface(surface);
            if (surface.TryGetFootstepIndex(playerControllerB.hit.point, checkStandingOnTerrain, out int footstepSurfaceIndex, playerControllerB))
            {
                currentFootstepSurfaceIndex = footstepSurfaceIndex;
            }

            DawnSurfaceInfo surfaceInfo = StartOfRound.Instance.footstepSurfaces[currentFootstepSurfaceIndex].GetDawnInfo();
            if (surfaceInfo.SurfaceVFXPrefab != null)
            {
                FootstepVFXPool.Instance!.Play(surfaceInfo.SurfaceVFXPrefab, hit.point, hit.normal, surfaceInfo.SurfaceVFXOffset, 1f);
            }

            return true; // DawnSurface found, return early!
        }

        playerControllerB.SetCurrentDawnSurface(null);
        return false; // Vanilla surface, do nothin'.
    }

    private static bool TryGetAndSetDawnSurfaceIndexMasked(MaskedPlayerEnemy maskedPlayerEnemy, ref RaycastHit hit, ref int currentFootstepSurfaceIndex)
    {
        if (hit.collider.TryGetComponent(out DawnSurface surface) && surface.SurfaceIndex > -1)
        {
            maskedPlayerEnemy.SetCurrentDawnSurface(surface);
            if (surface.TryGetFootstepIndex(hit.point, false, out int footstepSurfaceIndex))
            {
                currentFootstepSurfaceIndex = footstepSurfaceIndex;
            }

            DawnSurfaceInfo surfaceInfo = StartOfRound.Instance.footstepSurfaces[currentFootstepSurfaceIndex].GetDawnInfo();
            if (surfaceInfo.SurfaceVFXPrefab != null)
            {
                FootstepVFXPool.Instance!.Play(surfaceInfo.SurfaceVFXPrefab, hit.point, hit.normal, surfaceInfo.SurfaceVFXOffset, 1f);
            }
            return true; // DawnSurface found, return early!
        }

        maskedPlayerEnemy.SetCurrentDawnSurface(null);
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