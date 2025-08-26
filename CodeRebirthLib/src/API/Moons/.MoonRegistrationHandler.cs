using System.Collections.Generic;
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
            if (level.TryGetCRInfo(out _))
                continue;

            Debuggers.Moons?.Log($"Registering potentially modded level: {level.PlanetName}");
            NamespacedKey<CRMoonInfo> key;
            if (LLLCompat.Enabled && LLLCompat.IsExtendedLevel(level))
            {
                key = NamespacedKey<CRMoonInfo>.From("lethal_level_loader", NamespacedKey.NormalizeStringForNamespacedKey(level.PlanetName, false));
            }
            else
            {
                key = NamespacedKey<CRMoonInfo>.From("unknown_modded", NamespacedKey.NormalizeStringForNamespacedKey(level.PlanetName, false));
            }

            List<NamespacedKey> tags = [CRLibTags.IsExternal];

            if (LLLCompat.Enabled && LLLCompat.TryGetAllTagsWithModNames(level, out List<(string modName, string tagName)> tagsWithModNames))
            {
                foreach ((string modName, string tagName) in tagsWithModNames)
                {
                    bool alreadyAdded = false;
                    foreach (NamespacedKey tag in tags)
                    {
                        if (tag.Key == tagName)
                        {
                            alreadyAdded = true;
                            break;
                        }
                    }

                    if (alreadyAdded)
                        continue;

                    string normalizedModName = NamespacedKey.NormalizeStringForNamespacedKey(modName, false);
                    string normalizedTagName = NamespacedKey.NormalizeStringForNamespacedKey(tagName, false);
                    Debuggers.Moons?.Log($"Adding tag {normalizedModName}:{normalizedTagName} to level {level.PlanetName}");
                    tags.Add(NamespacedKey.From(normalizedModName, normalizedTagName));
                }
            }
            CRMoonInfo moonInfo = new(key, tags, level);
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

        CRMoonInfo testMoonInfo = new(MoonKeys.Test, [CRLibTags.IsExternal], self.currentLevel);
        self.currentLevel.SetCRInfo(testMoonInfo);
        LethalContent.Moons.Register(testMoonInfo);

        foreach (SelectableLevel level in self.levels)
        {
            NamespacedKey<CRMoonInfo>? key = (NamespacedKey<CRMoonInfo>?)typeof(MoonKeys).GetField(NamespacedKey.NormalizeStringForNamespacedKey(level.PlanetName, true).RemoveEnd("Level"))?.GetValue(null);
            if (key == null)
                continue;

            List<NamespacedKey> tags = [CRLibTags.IsExternal];

            if (LLLCompat.Enabled && LLLCompat.TryGetAllTagsWithModNames(level, out List<(string modName, string tagName)> tagsWithModNames))
            {
                foreach ((string modName, string tagName) in tagsWithModNames)
                {
                    bool alreadyAdded = false;
                    foreach (NamespacedKey tag in tags)
                    {
                        if (tag.Key == tagName)
                        {
                            alreadyAdded = true;
                            break;
                        }
                    }

                    if (alreadyAdded)
                        continue;

                    string normalizedModName = NamespacedKey.NormalizeStringForNamespacedKey(modName, false);
                    string normalizedTagName = NamespacedKey.NormalizeStringForNamespacedKey(tagName, false);
                    Debuggers.Moons?.Log($"Adding tag {normalizedModName}:{normalizedTagName} to level {level.PlanetName}");
                    tags.Add(NamespacedKey.From(normalizedModName, normalizedTagName));
                }
            }
            CRMoonInfo moonInfo = new(key, tags, level);
            level.SetCRInfo(moonInfo);
            LethalContent.Moons.Register(moonInfo);
        }
        orig(self);
    }
}