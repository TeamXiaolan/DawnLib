using System.Reflection;
using BepInEx.Bootstrap;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using TerminalFormatter;

namespace Dawn.Internal;

static class TerminalFormatterCompat
{
    public static bool Enabled => Chainloader.PluginInfos.ContainsKey(PluginInfo.PLUGIN_GUID);
    internal static bool alreadyPatched = false;

    internal static void Init()
    {
        // teehee maxxing :3
        alreadyPatched = true;
        MethodInfo getNodeText = AccessTools.DeclaredMethod(typeof(TerminalFormatter.Nodes.Store), "GetNodeText");
        if (getNodeText != null)
        {
            DawnPlugin.ILHooks.Add(new ILHook(getNodeText, TerminalPatches.UseFailedNameResults));
        }
        else
        {
            DawnPlugin.Logger.LogWarning("[TerminalFormatterCompat] Could not find Store.GetNodeText — TerminalFormatter compat disabled.");
        }
    }
}