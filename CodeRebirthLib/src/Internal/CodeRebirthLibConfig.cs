using CodeRebirthLib.CRMod;
using CodeRebirthLib.Internal.ModCompats;
using CodeRebirthLib.Utils;

namespace CodeRebirthLib.Internal;
static class CodeRebirthLibConfig
{
    public static CompatibilityBool LethalConfigCompatibility;

    public static bool CreateTagExport;

    internal static void Bind(ConfigManager manager)
    {
        using (ConfigContext context = manager.CreateConfigSection("Compatibility"))
        {
            LethalConfigCompatibility = context.Bind("Extend LethalConfig Support", $"Patches LethalConfig to enable raw editing of strings for unknown types.\nCurrent Targeted Version: {LethalConfigCompat.VERSION}", CompatibilityBool.IfVersionMatches).Value;
        }

        using(ConfigContext context = manager.CreateConfigSection("Exports"))
        {
            CreateTagExport = context.Bind("Tag Info Export", "Export a markdown file listing all tags?", false).Value;
        }
    }
}