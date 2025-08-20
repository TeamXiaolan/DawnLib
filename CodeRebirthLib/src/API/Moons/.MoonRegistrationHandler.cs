using CodeRebirthLib.Internal;
using CodeRebirthLib.Internal.ModCompats;
using EasyTextEffects.Editor.MyBoxCopy.Extensions;
using MonoMod.RuntimeDetour;

namespace CodeRebirthLib;

static class MoonRegistrationHandler
{
    internal static void Init()
    {
        using (new DetourContext(priority: int.MaxValue))
        {
            On.StartOfRound.Awake += CollectLevels;
            On.StartOfRound.Start += CollectLevels;
        }
    }

    private static void CollectLevels(On.StartOfRound.orig_Start orig, StartOfRound self)
    {
        orig(self);

        if (LethalContent.Moons.IsFrozen)
            return;

        foreach (SelectableLevel level in self.levels)
        {
            if (level.HasCRInfo())
                continue;

            Debuggers.Moons?.Log($"Registering potentially modded level: {level.PlanetName}");
            NamespacedKey<CRMoonInfo> key;
            if (LLLCompat.Enabled && LLLCompat.TryGetExtendedLevel(level, out _))
            {
                key = NamespacedKey<CRMoonInfo>.From("lethal_level_loader", NamespacedKey.NormalizeStringForNamespacedKey(level.PlanetName, false));
            }
            else
            {
                key = NamespacedKey<CRMoonInfo>.From("unknown_modded", NamespacedKey.NormalizeStringForNamespacedKey(level.PlanetName, false));
            }
            CRMoonInfo moonInfo = new(key, [CRLibTags.IsExternal], level);
            level.SetCRInfo(moonInfo);
            LethalContent.Moons.Register(moonInfo);
        }
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
            NamespacedKey<CRMoonInfo>? key = (NamespacedKey<CRMoonInfo>?)typeof(MoonKeys).GetField(NamespacedKey.NormalizeStringForNamespacedKey(level.PlanetName, true).RemoveEnd("Level"))?.GetValue(null);
            if (key == null)
                continue;

            if (LethalContent.Moons.ContainsKey(key))
                continue;

            CRMoonInfo moonInfo = new(key, [CRLibTags.IsExternal], level);
            level.SetCRInfo(moonInfo);
            LethalContent.Moons.Register(moonInfo);
        }
        orig(self);
    }
}