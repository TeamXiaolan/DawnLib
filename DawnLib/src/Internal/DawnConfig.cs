using BepInEx.Configuration;
using Dawn.Utils;

namespace Dawn.Internal;
static class DawnConfig
{
    public static CompatibilityBool LethalConfigCompatibility;

    public static bool CreateTagExport;

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
    }
}