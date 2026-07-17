using Dawn.Utils;
using UnityEngine;

namespace Dawn;

static class DeadBodyRegistrationHandler
{
    internal static void Init()
    {
        On.StartOfRound.Awake += CollectVanillaDeadBodies;
    }

    private static void CollectVanillaDeadBodies(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        if (LethalContent.DeadBodies.IsFrozen)
        {
            orig(self);
        }

        foreach ((int index, GameObject ragdollPrefab) in self.playerRagdolls.WithIndex())
        {
            string name = ragdollPrefab.name;
            NamespacedKey<DawnDeadBodyInfo>? key = DeadBodyKeys.GetByReflection(name);
            if (key == null)
            {
                key = NamespacedKey<DawnDeadBodyInfo>.From("unknown_modded", name);
            }

            DawnDeadBodyInfo dawnBodyInfo = new DawnDeadBodyInfo(key, [], ragdollPrefab, null)
            {
                Index = index
            };

            DawnDeadBodyNamespacedKeyContainer container = ragdollPrefab.AddComponent<DawnDeadBodyNamespacedKeyContainer>();
            container.Value = dawnBodyInfo.Key;

            if (ragdollPrefab.TryGetComponent(out DeadBodyInfo deadBodyInfo))
            {
                deadBodyInfo.DawnInfo = dawnBodyInfo;
            }

            LethalContent.DeadBodies.Register(dawnBodyInfo);
        }

        foreach (DawnDeadBodyInfo dawnBodyInfo in LethalContent.DeadBodies.Values)
        {
            if (dawnBodyInfo.ShouldSkipIgnoreOverride())
            {
                continue;
            }

            int index = self.playerRagdolls.Count;
            self.playerRagdolls.Add(dawnBodyInfo.DeadBodyPrefab);
            dawnBodyInfo.Index = index;
        }

        LethalContent.DeadBodies.Freeze();
        orig(self);
    }
}