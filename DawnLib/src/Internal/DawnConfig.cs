using BepInEx.Configuration;
using Dawn.Utils;
using static Dawn.Utils.BepinexUtils;

namespace Dawn.Internal;
static class DawnConfig
{
    public static CompatibilityBool LethalConfigCompatibility;

    public static bool CreateTagExport;

    public static bool DisableDawnItemSaving;

    public static bool DisableDawnUnlockableSaving;

    public static bool DisableAchievementsButton;

    public static bool AllowLLLToOverrideVanillaStatus;

    public static ConfigEntry<bool> TerminalKeywordResolution;
    public static ConfigEntry<int> TerminalKeywordSpecificity;

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

        DisableAchievementsButton = file.Bind(
            "Achievements",
            "Disable Achievements Button",
            false,
            "Disable the Achievements Button from showing up in the main menu"
        ).Value;

        AllowLLLToOverrideVanillaStatus = file.Bind(
            "Compatibility",
            "Allow LLL to Override Vanilla Moon Locked/Hidden Status",
            false,
            "Allow LLL to override the vanilla status of unlockables"
        ).Value;

        //example of CreateConfigItem usage that uses generics and accepts min/max values automatically
        TerminalKeywordResolution = CreateConfigItem(file,
            "Terminal",
            "Keyword Resolution",
            true,
            "Dawnlib's terminal keyword resolution sytem to better handle conflicting keywords.");
        TerminalKeywordSpecificity = CreateConfigItem(file,
            "Terminal",
            "Keyword Specificity",
            3,
            "When Keyword Resolution is enabled, how many characters must match for a keyword to be considered a result for a given input in the terminal.",
            0, //min value
            5); //max value
    }
}