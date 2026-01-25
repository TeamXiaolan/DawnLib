using BepInEx.Configuration;
using Dawn.Utils;

namespace Dawn.Internal;
static class DawnConfig
{
    public static ConfigEntry<CompatibilityBool> LethalConfigCompatibility;

    public static ConfigEntry<bool> CreateTagExport;

    public static ConfigEntry<bool> DisableDawnItemSaving;

    public static ConfigEntry<bool> DisableDawnUnlockableSaving;
    public static ConfigEntry<bool> DisableAchievementsButton;

    public static ConfigEntry<bool> AllowLLLToOverrideVanillaStatus;

    public static ConfigEntry<bool> TerminalKeywordResolution;
    public static ConfigEntry<int> TerminalKeywordSpecificity;

    internal static void Bind(ConfigFile file)
    {
        LethalConfigCompatibility = file.CleanedBind(
            "Compatibility",
            "Extend LethalConfig Support",
            CompatibilityBool.IfVersionMatches,
            $"Patches LethalConfig to enable raw editing of strings for unknown types.\nCurrent Targeted Version: {LethalConfigCompat.VERSION}"
        );

        CreateTagExport = file.CleanedBind(
            "Exports",
            "Tag Info Export",
            false,
            "Export a markdown file listing all tags?"
        );

        DisableDawnItemSaving = file.CleanedBind(
            "Dawn Save System",
            "Item Saving",
            false,
            "Disable the Dawn Save System for item saving"
        );

        DisableDawnUnlockableSaving = file.CleanedBind(
            "Dawn Save System",
            "Unlockable Saving",
            false,
            "Disable the Dawn Save System for unlockable saving"
        );

        DisableAchievementsButton = file.CleanedBind(
            "Achievements",
            "Disable Achievements Button",
            false,
            "Disable the Achievements Button from showing up in the main menu"
        );

        AllowLLLToOverrideVanillaStatus = file.CleanedBind(
            "Compatibility",
            "Allow LLL to Override Vanilla Moon Locked/Hidden Status",
            false,
            "Allow LLL to override the vanilla status of unlockables"
        );

        //example of CreateConfigItem usage that uses generics and accepts min/max values automatically
        TerminalKeywordResolution = file.CleanedBind(
            "Terminal",
            "Keyword Resolution",
            true,
            "Dawnlib's terminal keyword resolution sytem to better handle conflicting keywords.");
        TerminalKeywordSpecificity = file.CleanedBind(
            "Terminal",
            "Keyword Specificity",
            3,
            "When Keyword Resolution is enabled, how many characters must match for a keyword to be considered a result for a given input in the terminal.",
            0, //min value
            5); //max value
    }
}