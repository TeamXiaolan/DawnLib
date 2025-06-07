using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.Data;
using CodeRebirthLib.Patches;

namespace CodeRebirthLib;
static class CodeRebirthLibConfig
{
    public static CompatibilityBool LethalConfigCompatibility;
    public static bool ExtendedLogging;

    internal static void Bind(ConfigManager manager)
    {
        using(ConfigContext context = manager.CreateConfigSection("Compatibility"))
        {
            LethalConfigCompatibility = context.Bind("Extend LethalConfig Support", $"Patches LethalConfig to enable raw editing of strings for unknown types.\nCurrent Targeted Version: {LethalConfigPatch.VERSION}", CompatibilityBool.IfVersionMatches).Value;
        }

        using(ConfigContext context = manager.CreateConfigSection("Debugging"))
        {
            ExtendedLogging = context.Bind("Extended Logging", "Enable debug logs", false).Value;
        }
    }
}