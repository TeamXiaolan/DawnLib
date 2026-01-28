using BepInEx.Bootstrap;

namespace Dawn.Internal;
static class MoonDaySpeedMultiplierPatcherCompat
{
    public static bool Enabled => Chainloader.PluginInfos.ContainsKey("com.github.WhiteSpike.MoonDaySpeedMultiplierPatcher");
}