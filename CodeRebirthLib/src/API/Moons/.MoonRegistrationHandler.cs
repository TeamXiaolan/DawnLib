using System;
using System.Collections;
using MonoMod.RuntimeDetour;

namespace CodeRebirthLib;

static class MoonRegistrationHandler
{
    internal static void Init()
    {
        using (new DetourContext(priority: int.MaxValue))
        {
            On.StartOfRound.Awake += CollectLevels;
        }
    }

    private static IEnumerator FreezeLevels(StartOfRound self)
    {
        yield return null;
        yield return null;
        LethalContent.Moons.Freeze();
    }

    private static void CollectLevels(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        if (LethalContent.Moons.IsFrozen)
        {
            orig(self);
            return;
        }

        foreach (SelectableLevel level in self.levels)
        {
            NamespacedKey<CRMoonInfo>? key = (NamespacedKey<CRMoonInfo>?)typeof(MoonKeys).GetField(NamespacedKey.NormalizeStringForNamespacedKey(level.PlanetName, true))?.GetValue(null);
            if (key == null)
                continue;

            if (LethalContent.Moons.ContainsKey(key))
                continue;

            CRMoonInfo moonInfo = new(key, [CRLibTags.IsExternal], level);
            level.SetCRInfo(moonInfo);
            LethalContent.Moons.Register(moonInfo);
        }
        orig(self);
        self.StartCoroutine(FreezeLevels(self));
    }
}