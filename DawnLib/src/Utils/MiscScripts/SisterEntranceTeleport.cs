using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace Dawn.Utils;

[AddComponentMenu($"{DawnConstants.MiscUtils}/Sister EntranceTeleport")]
public class SisterEntranceTeleport : MonoBehaviour
{
    [field: SerializeField]
    public AudioClip[] CreakDoorOpenClips { get; private set; } = [];

    [field: SerializeField]
    public AudioClip[] ShutDoorClips { get; private set; } = [];

    public static void Init()
    {
        IL.EntranceTeleport.PlayCreakSFX += ApplySisterChangesCreak;
        IL.EntranceTeleport.PlayAudioAtTeleportPositions += ApplySisterChangesShut;
    }

    private static void ApplySisterChangesCreak(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        ILLabel markedLabel = null!;
        if (!cursor.TryGotoNext(
            MoveType.After,
            il => il.MatchBr(out markedLabel),
            il => il.MatchCall<StartOfRound>("get_Instance"),
            il => il.MatchLdfld<StartOfRound>(nameof(StartOfRound.creakOpenDoorMetal)),
            il => il.MatchStloc(0)
        ))
        {
            DawnPlugin.Logger.LogError($"Couldn't match EntranceTeleport.PlayCreakSFX IL (1).");
            return;
        }

        cursor.MarkLabel(markedLabel);
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldc_I4_0);
        cursor.Emit(OpCodes.Ldloc_0);
        cursor.EmitDelegate(GetReplacementClips);
        cursor.Emit(OpCodes.Stloc_0);

        ILLabel secondMarkedLabel = null!;
        if (!cursor.TryGotoNext(
            MoveType.After,
            il => il.MatchBr(out secondMarkedLabel),
            il => il.MatchCall<StartOfRound>("get_Instance"),
            il => il.MatchLdfld<StartOfRound>(nameof(StartOfRound.creakOpenDoorMetal)),
            il => il.MatchStloc(0)
        ))
        {
            DawnPlugin.Logger.LogError($"Couldn't match EntranceTeleport.PlayCreakSFX IL (2).");
            return;
        }

        cursor.MarkLabel(secondMarkedLabel);
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldc_I4_0);
        cursor.Emit(OpCodes.Ldloc_0);
        cursor.EmitDelegate(GetReplacementClips);
        cursor.Emit(OpCodes.Stloc_0);
    }

    private static void ApplySisterChangesShut(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        ILLabel markedLabel = null!;
        if (!cursor.TryGotoNext(
            MoveType.After,
            il => il.MatchStloc(0),
            il => il.MatchLdarg(0),
            il => il.MatchLdfld<EntranceTeleport>(nameof(EntranceTeleport.isEntranceToBuilding)),
            il => il.MatchBrfalse(out markedLabel)
        ))
        {
            DawnPlugin.Logger.LogError($"Couldn't match EntranceTeleport.PlayAudioAtTeleportPositions IL (1).");
            return;
        }

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitLdfld<EntranceTeleport>(nameof(EntranceTeleport.exitScript));
        cursor.Emit(OpCodes.Ldc_I4_1);
        cursor.Emit(OpCodes.Ldloc_0);
        cursor.EmitDelegate(GetReplacementClips);
        cursor.Emit(OpCodes.Stloc_0);

        if (!cursor.TryGotoNext(
            MoveType.After,
            il => il.MatchLdloc(1),
            il => il.MatchLdlen(),
            il => il.MatchConvI4(),
            il => il.MatchCall(out _),
            il => il.MatchLdelemRef(),
            il => il.MatchCallvirt(out _)
        ))
        {
            DawnPlugin.Logger.LogError($"Couldn't match EntranceTeleport.PlayAudioAtTeleportPositions IL (2).");
            return;
        }

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldc_I4_1);
        cursor.Emit(OpCodes.Ldloc_1);
        cursor.EmitDelegate(GetReplacementClips);
        cursor.Emit(OpCodes.Stloc_1);

        if (!cursor.TryGotoNext(
            MoveType.After,
            il => il.MatchLdelemRef(),
            il => il.MatchCallvirt(out _),
            il => il.MatchRet()
        ))
        {
            DawnPlugin.Logger.LogError($"Couldn't match EntranceTeleport.PlayAudioAtTeleportPositions IL (3).");
            return;
        }

        cursor.MarkLabel(markedLabel);
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldc_I4_1);
        cursor.Emit(OpCodes.Ldloc_1);
        cursor.EmitDelegate(GetReplacementClips);
        cursor.Emit(OpCodes.Stloc_1);

        if (!cursor.TryGotoNext(
            MoveType.After,
            il => il.MatchLdloc(0),
            il => il.MatchLdlen(),
            il => il.MatchConvI4(),
            il => il.MatchCall(out _),
            il => il.MatchLdelemRef(),
            il => il.MatchCallvirt(out _)
        ))
        {
            DawnPlugin.Logger.LogError($"Couldn't match EntranceTeleport.PlayAudioAtTeleportPositions IL (4).");
            return;
        }

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitLdfld<EntranceTeleport>(nameof(EntranceTeleport.exitScript));
        cursor.Emit(OpCodes.Ldc_I4_1);
        cursor.Emit(OpCodes.Ldloc_0);
        cursor.EmitDelegate(GetReplacementClips);
        cursor.Emit(OpCodes.Stloc_0);
    }

    private static AudioClip[] GetReplacementClips(EntranceTeleport entranceTeleport, bool shut, AudioClip[] originalClips)
    {
        if (entranceTeleport.TryGetComponent(out SisterEntranceTeleport sisterEntranceTeleport))
        {
            if (shut)
            {
                return sisterEntranceTeleport.ShutDoorClips;
            }

            return sisterEntranceTeleport.CreakDoorOpenClips;
        }

        return originalClips;
    }
}