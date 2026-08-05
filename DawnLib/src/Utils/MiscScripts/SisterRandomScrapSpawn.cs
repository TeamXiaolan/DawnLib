using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace Dawn.Utils;

[AddComponentMenu($"{DawnConstants.MiscUtils}/Sister RandomScrapSpawn")]
public class SisterRandomScrapSpawn : MonoBehaviour
{
    [field: SerializeField]
    public bool SpawnedItemsCopyRotation { get; private set; }

    [field: SerializeField]
    public bool RemoveNoItemGroupItemSpawning { get; private set; }

    public static void Init()
    {
        IL.RoundManager.SpawnScrapInLevel += ApplySisterChanges;
    }

    private static void ApplySisterChanges(ILContext il)
    {
        ILCursor cursor = new(il);
        if (!cursor.TryGotoNext(
            MoveType.After,
            il => il.MatchBeq(out _),
            il => il.MatchLdloc(8),
            il => il.MatchCall(out _),
            il => il.MatchStloc(9),
            il => il.MatchBr(out _)
        ))
        {
            DawnPlugin.Logger.LogError($"Couldn't match RoundManager.SpawnScrapInLevel IL (1).");
            return;
        }

        cursor.Emit(OpCodes.Ldloc, 9);
        cursor.Emit(OpCodes.Ldloc, 2);
        cursor.EmitDelegate((List<RandomScrapSpawn> randomScrapSpawns, int sidNumber) =>
        {
            if (sidNumber != -1)
            {
                return;
            }

            for (int i = randomScrapSpawns.Count - 1; i >= 0; i--)
            {
                RandomScrapSpawn randomScrapSpawn = randomScrapSpawns[i];
                SisterRandomScrapSpawn? sisterRandomScrapSpawn = randomScrapSpawn.GetComponent<SisterRandomScrapSpawn>();
                if (sisterRandomScrapSpawn != null && sisterRandomScrapSpawn.RemoveNoItemGroupItemSpawning)
                {
                    randomScrapSpawns.RemoveAt(i);
                }
            }
        });

        if (!cursor.TryGotoNext(
            MoveType.Before,
            il => il.MatchLdloc(18),
            il => il.MatchCallvirt<UnityEngine.Component>("get_transform"),
            il => il.MatchLdloc(18),
            il => il.MatchLdfld<GrabbableObject>(nameof(GrabbableObject.itemProperties)),
            il => il.MatchLdfld<Item>(nameof(Item.restingRotation)),
            il => il.MatchCall<UnityEngine.Quaternion>("Euler"),
            il => il.MatchCallvirt<UnityEngine.Transform>("set_rotation")
        ))
        {
            DawnPlugin.Logger.LogError($"Couldn't match RoundManager.SpawnScrapInLevel IL (2).");
            return;
        }

        cursor.Index += 2;
        cursor.RemoveRange(3);
        cursor.Emit(OpCodes.Ldloc, 7);
        cursor.Emit(OpCodes.Ldloc, 18);
        cursor.EmitDelegate((RandomScrapSpawn randomScrapSpawn, GrabbableObject grabbableObject) =>
        {
            if (randomScrapSpawn.TryGetComponent(out SisterRandomScrapSpawn sisterRandomScrapSpawn) && sisterRandomScrapSpawn.SpawnedItemsCopyRotation)
            {
                return randomScrapSpawn.transform.rotation.eulerAngles;
            }
            else
            {
                return grabbableObject.itemProperties.restingRotation;
            }
        });
    }
}