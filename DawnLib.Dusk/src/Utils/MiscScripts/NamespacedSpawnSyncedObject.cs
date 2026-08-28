using System;
using Dawn;
using Dawn.Internal;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace Dusk.Utils;

[AddComponentMenu($"{DuskModConstants.NetworkingComponents}/Namespaced SpawnSyncedObject")]
public class NamespacedSpawnSyncedObject : MonoBehaviour
{
    [field: SerializeField]
    public NamespacedKey SyncedObjectKey { get; private set; }

    internal GameObject? resolvedPrefab = null;

    internal GameObject? ResolvePrefab()
    {
        foreach (DuskNamespacedObjectDefinition namespacedObjectDefinition in DuskModContent.NamespacedObjects.Values)
        {
            if (namespacedObjectDefinition.TypedKey != SyncedObjectKey)
                continue;

            resolvedPrefab = namespacedObjectDefinition.NamespacedObject;
            return namespacedObjectDefinition.NamespacedObject;
        }

        return null;
    }

    internal static void Init()
    {
        IL.RoundManager.SpawnSyncedProps += ReplaceSpawnSyncedPrefab;
    }

    private static void ReplaceSpawnSyncedPrefab(ILContext il)
    {
        ILCursor cursor = new(il);
        if (!cursor.TryGotoNext(
            MoveType.Before,
            il => il.MatchLdloc(0),
            il => il.MatchLdloc(3),
            il => il.MatchLdelemRef(),
            il => il.MatchLdfld<SpawnSyncedObject>(nameof(SpawnSyncedObject.spawnPrefab)),
            il => il.MatchLdloc(0),
            il => il.MatchLdloc(3)
        ))
        {
            DuskPlugin.Logger.LogError($"Couldn't match RoundManager.SpawnSyncedProps IL (1).");
            return;
        }

        cursor.Emit(OpCodes.Ldloc_0);
        cursor.Emit(OpCodes.Ldloc_3);
        cursor.EmitDelegate(HandleNamespacedSpawnSyncedObject);
        // Get inside the array loop, check for the spawnPrefab for the NamespacedSpawnSyncedObject component and use that to replace the prefab IF resolvePrefab isn't null.
    }

    private static void HandleNamespacedSpawnSyncedObject(SpawnSyncedObject[] spawnSyncedObjects, int index)
    {
        SpawnSyncedObject spawnSyncedObject = spawnSyncedObjects[index];
        if (spawnSyncedObject.spawnPrefab != null && spawnSyncedObject.spawnPrefab.TryGetComponent(out NamespacedSpawnSyncedObject namespacedSpawnSyncedObject))
        {
            GameObject? resolvedPrefab = namespacedSpawnSyncedObject.ResolvePrefab();
            if (resolvedPrefab != null)
            {
                Debuggers.NamespacedObjects?.Log($"Resolved prefab for NamespacedSpawnSyncedObject: {namespacedSpawnSyncedObject.SyncedObjectKey}");
                spawnSyncedObject.spawnPrefab = resolvedPrefab;
            }
        }
    }
}