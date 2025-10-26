using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;

namespace Dawn.Internal;
static class MoreSuitsCompat
{
    public static bool Enabled => Chainloader.PluginInfos.ContainsKey("x753.More_Suits");

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static void ForceMoreSuitsRegistration()
    {
    }
}