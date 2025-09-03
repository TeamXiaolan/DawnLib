using Dawn.Dusk;
using Dawn.Internal;
using Dawn.Utils;

namespace Dawn.Internal;
static class DawnConfig
{
    public static CompatibilityBool LethalConfigCompatibility;

    public static bool CreateTagExport;

    internal static void Bind(ConfigManager manager)
    {
        using (ConfigContext context = manager.CreateConfigSection("Compatibility"))
        {
            LethalConfigCompatibility = context.Bind("Extend LethalConfig Support", $"Patches LethalConfig to enable raw editing of strings for unknown types.\nCurrent Targeted Version: {LethalConfigCompat.VERSION}", CompatibilityBool.IfVersionMatches).Value;
        }

        using (ConfigContext context = manager.CreateConfigSection("Exports"))
        {
            CreateTagExport = context.Bind("Tag Info Export", "Export a markdown file listing all tags?", false).Value;
        }
    }
}