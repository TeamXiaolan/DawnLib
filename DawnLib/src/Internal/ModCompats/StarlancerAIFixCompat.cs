using BepInEx.Bootstrap;

namespace Dawn.Internal;

static class StarlancerAIFixCompat
{
    public static bool Enabled => Chainloader.PluginInfos.ContainsKey(StarlancerAIFix.StarlancerAIFixBase.modGUID);
}