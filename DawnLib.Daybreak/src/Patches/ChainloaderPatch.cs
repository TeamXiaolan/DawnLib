using BepInEx.Bootstrap;
using Daybreak;
using HarmonyLib;
using System;
using System.Reflection;

namespace LethalPerformance.Patcher.Patches;

[HarmonyPatch(typeof(Chainloader))]
internal static class Patch_Chainloader
{
    [HarmonyPatch(nameof(Chainloader.Initialize))]
    [HarmonyPostfix]
    private static void Initialize()
    {
        try
        {
            DaybreakPlugin.Harmony!.Patch(MethodOf(Chainloader.Start), postfix: new(MethodOf(ModsLoaded)));
        }
        catch (Exception e)
        {
            DaybreakPlugin.Log.LogWarning(e);
        }
    }

    private static void ModsLoaded()
    {
        DaybreakPlugin.OnModsLoaded.Invoke();
    }

    private static MethodInfo MethodOf(Delegate @delegate) => @delegate.Method;
}