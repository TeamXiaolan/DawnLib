using BepInEx.Bootstrap;
using TerminalFormatter;

namespace Dawn.Internal;
static class TerminalFormatterCompat
{
    public static bool Enabled => Chainloader.PluginInfos.ContainsKey(PluginInfo.PLUGIN_GUID);

    internal static void Init()
    {
        // teehee maxxing :3
        IL.TerminalFormatter.Nodes.Store.GetNodeText += TerminalPredicatePatch.UseFailedResultName;
    }
}