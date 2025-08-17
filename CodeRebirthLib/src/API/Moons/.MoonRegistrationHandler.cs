using System.Linq;

namespace CodeRebirthLib;

static class MoonRegistrationHandler
{
    internal static void Init()
    {
        On.StartOfRound.Awake += CollectLevels;
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
            NamespacedKey<CRMoonInfo>? key = (NamespacedKey<CRMoonInfo>?)typeof(MoonKeys).GetField(new string(level.PlanetName.Replace(" ", "").SkipWhile(c => !char.IsLetter(c)).ToArray()))?.GetValue(null);
            if (key == null)
                continue;

            if (LethalContent.Moons.ContainsKey(key))
                continue;

            CRMoonInfo moonInfo = new(key, level);
            level.SetCRInfo(moonInfo);
            LethalContent.Moons.Register(moonInfo);
        }

        LethalContent.Moons.Freeze();
        orig(self);
    }
}