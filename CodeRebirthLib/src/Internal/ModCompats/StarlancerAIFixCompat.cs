using BepInEx.Bootstrap;

namespace CodeRebirthLib.Internal.ModCompats;

static class StarlancerAIFixCompat
{
    public static bool Enabled => Chainloader.PluginInfos.ContainsKey(StarlancerAIFix.StarlancerAIFixBase.modGUID);
}