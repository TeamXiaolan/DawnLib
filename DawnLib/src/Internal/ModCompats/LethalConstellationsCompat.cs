using BepInEx.Bootstrap;

namespace Dawn.Internal;

static class LethalConstellationsCompat
{
    public static bool Enabled => Chainloader.PluginInfos.ContainsKey("com.github.darmuh.LethalConstellations");
}