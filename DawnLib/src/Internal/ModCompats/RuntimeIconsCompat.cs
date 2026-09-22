using BepInEx.Bootstrap;

namespace Dawn.Internal;

static class RuntimeIconsCompat
{
    public static bool Enabled => Chainloader.PluginInfos.ContainsKey("com.github.lethalcompanymodding.runtimeicons");
}