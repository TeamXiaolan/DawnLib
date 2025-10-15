using BepInEx.Configuration;
using Dawn.Utils;

namespace Dawn.Internal;
static class DawnConfig
{
    public static CompatibilityBool LethalConfigCompatibility;

    public static bool CreateTagExport;

    public static bool DisableDawnItemSaving;

    public static bool DisableDawnUnlockableSaving;

    internal static void Bind(ConfigFile file)
    {
        LethalConfigCompatibility = file.Bind(
            "Compatibility",
            "Extend LethalConfig Support",
            CompatibilityBool.IfVersionMatches,
            $"Patches LethalConfig to enable raw editing of strings for unknown types.\nCurrent Targeted Version: {LethalConfigCompat.VERSION}"
        ).Value;

        CreateTagExport = file.Bind(
            "Exports",
            "Tag Info Export",
            false,
            "Export a markdown file listing all tags?"
        ).Value;

        DisableDawnItemSaving = file.Bind(
            "Dawn Save System",
            "Item Saving",
            false,
            "Disable the Dawn Save System for item saving"
        ).Value;

        DisableDawnUnlockableSaving = file.Bind(
            "Dawn Save System",
            "Unlockable Saving",
            false,
            "Disable the Dawn Save System for unlockable saving"
        ).Value;
    }
}