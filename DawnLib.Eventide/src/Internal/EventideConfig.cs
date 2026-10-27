using BepInEx.Configuration;
using Dawn.Utils;

namespace Eventide.Internal;

static class EventideConfig
{
    public static ConfigEntry<bool> DisableAchievementsButton;

    internal static void Bind(ConfigFile file)
    {
        DisableAchievementsButton = file.CleanedBind(
            "Achievements",
            "Disable Achievements Button",
            false,
            "Disable the Achievements Button from showing up in the main menu"
        );
    }
}