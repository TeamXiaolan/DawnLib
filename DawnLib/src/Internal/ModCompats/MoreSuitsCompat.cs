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
        // i didn't want to create an actual dependency on MoreSuits, so im just using straight up reflection.
        Chainloader.PluginInfos["x753.More_Suits"].Instance.GetType()
            .GetNestedType("StartOfRoundPatch", BindingFlags.NonPublic | BindingFlags.Default)
            .GetMethod("StartPatch", BindingFlags.Static | BindingFlags.NonPublic)
            .Invoke(null, [StartOfRound.Instance]);
    }
}