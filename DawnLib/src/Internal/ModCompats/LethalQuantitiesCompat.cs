using System;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using LethalQuantities.Objects;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using UnityEngine;
using TomlTypeConverter = On.BepInEx.Configuration.TomlTypeConverter;

namespace Dawn.Internal;
static class LethalQuantitiesCompat
{
    public static bool Enabled => Chainloader.PluginInfos.ContainsKey(LethalQuantities.PluginInfo.PLUGIN_GUID);

    private static bool _useLQConverter;
    private static TypeConverter _lqConverter;
    private static TypeConverter _dawnConverter;

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void Init()
    {
        _lqConverter = new AnimationCurveTypeConverter();
        _dawnConverter = ExtendedTOML.WrapConverter(new AnimationCurveConverter());

        DawnPlugin.Hooks.Add(new Hook(AccessTools.DeclaredMethod(typeof(LethalQuantities.Patches.RoundManagerPatch), "onStartPrefix"), MarkShouldUseLQConverter));
        On.BepInEx.Configuration.TomlTypeConverter.GetConverter += ReplaceAnimationCurveConverter;
    }
    private static TypeConverter ReplaceAnimationCurveConverter(TomlTypeConverter.orig_GetConverter orig, Type valuetype)
    {
        if (valuetype != typeof(AnimationCurve))
        {
            return orig(valuetype);
        }

        if (_useLQConverter)
        {
            return _lqConverter;
        }
        else
        {
            return _dawnConverter;
        }
    }

    private static void MarkShouldUseLQConverter(RuntimeILReferenceBag.FastDelegateInvokers.Action<RoundManager> orig, RoundManager __instance)
    {
        _useLQConverter = true;
        orig(__instance);
        _useLQConverter = false;
    }
}