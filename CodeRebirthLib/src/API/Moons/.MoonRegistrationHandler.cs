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
            NamespacedKey<CRMoonInfo> key = level.ToNamespacedKey();

            CRMoonInfo moonInfo = new(key, level);
            LethalContent.Moons.Register(moonInfo);
        }

        LethalContent.Moons.Freeze();
        orig(self);
    }
}