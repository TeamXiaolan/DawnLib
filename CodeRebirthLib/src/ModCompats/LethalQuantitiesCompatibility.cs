using System;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using CodeRebirthLib.ConfigManagement;
using CodeRebirthLib.ConfigManagement.Converters;
using LethalQuantities.Objects;
using On.LethalQuantities.Patches;
using UnityEngine;
using TomlTypeConverter = On.BepInEx.Configuration.TomlTypeConverter;

namespace CodeRebirthLib.ModCompats;
static class LethalQuantitiesCompatibility
{
    public static bool Enabled => Chainloader.PluginInfos.ContainsKey(LethalQuantities.PluginInfo.PLUGIN_GUID);

    private static bool _useLQConverter;
    private static TypeConverter _lqConverter;
    private static TypeConverter _crlibConverter;
    
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void Init()
    {
        _lqConverter = new AnimationCurveTypeConverter();
        _crlibConverter = ExtendedTOML.WrapCRLibConverter(new AnimationCurveConverter());
        
        On.LethalQuantities.Patches.RoundManagerPatch.onStartPrefix += MarkShouldUseLQConverter;
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
            return _crlibConverter;
        }
    }
    private static void MarkShouldUseLQConverter(RoundManagerPatch.orig_onStartPrefix orig, RoundManager __instance)
    {
        _useLQConverter = true;
        orig(__instance);
        _useLQConverter = false;
    }
}