using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace Dawn.Utils;

public class DawnSpawnDenialPoint : MonoBehaviour
{
    [field: SerializeField]
    public float SpawnDenialPointOutsideHazardDistanceBlocker { get; private set; } = 6f;

    [field: SerializeField]
    public float SpawnDenialPointEnemySpawningDistanceBlocker { get; private set; } = 16f;

    internal static void Init()
    {
        IL.RoundManager.PositionWithDenialPointsChecked += ApplyEnemyDistanceBlocker;
        IL.RoundManager.SpawnOutsideHazards += ApplyOutsideHazardDistanceBlocker;
    }

    private static void ApplyOutsideHazardDistanceBlocker(ILContext il)
    {
        int denialPointIndex = -1;
        ILCursor cursor = new(il);
        if (!cursor.TryGotoNext(
            MoveType.Before,
            il => il.MatchLdcR4(6),
            il => il.MatchAdd(),
            il => il.MatchBgeUn(out _),
            il => il.MatchLdcI4(1),
            il => il.MatchStloc(out _),
            il => il.MatchBr(out _),
            il => il.MatchLdloc(out denialPointIndex),
            il => il.MatchLdcI4(1),
            il => il.MatchAdd()
        ))
        {
            DawnPlugin.Logger.LogWarning("Failed to apply RoundManager.PositionWithDenialPointsChecked patch");
            return;
        }

        cursor.Remove();
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitVanillaLdfld("RoundManager", nameof(RoundManager.spawnDenialPoints));
        cursor.EmitLdloc(denialPointIndex);
        cursor.Emit(OpCodes.Ldelem_Ref);
        cursor.EmitDelegate((GameObject spawnDenialPoint) =>
        {
            if (!spawnDenialPoint.TryGetComponent(out DawnSpawnDenialPoint dawnSpawnDenialPoint))
            {
                return 6f;
            }

            return dawnSpawnDenialPoint.SpawnDenialPointOutsideHazardDistanceBlocker;
        });
    }

    private static void ApplyEnemyDistanceBlocker(ILContext il)
    {
        ILCursor cursor = new(il);
        if (!cursor.TryGotoNext(
            MoveType.Before,
            il => il.MatchLdcR4(16),
            il => il.MatchBlt(out _),
            il => il.MatchLdarg(4),
            il => il.MatchLdcR4(-1),
            il => il.MatchBeq(out _),
            il => il.MatchLdarg(1)
        ))
        {
            DawnPlugin.Logger.LogWarning("Failed to apply RoundManager.PositionWithDenialPointsChecked patch");
            return;
        }

        cursor.Remove();
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitVanillaLdfld("RoundManager", nameof(RoundManager.spawnDenialPoints));
        cursor.EmitLdloc(3);
        cursor.Emit(OpCodes.Ldelem_Ref);
        cursor.EmitDelegate((GameObject spawnDenialPoint) =>
        {
            if (!spawnDenialPoint.TryGetComponent(out DawnSpawnDenialPoint dawnSpawnDenialPoint))
            {
                return 16f;
            }

            return dawnSpawnDenialPoint.SpawnDenialPointEnemySpawningDistanceBlocker;
        });
    }
}