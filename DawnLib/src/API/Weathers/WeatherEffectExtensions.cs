using System;
using System.Diagnostics.CodeAnalysis;
using CodeRebirthLib.Internal;
using Dawn.Preloader.Interfaces;

namespace CodeRebirthLib;

public static class WeatherEffectExtensions
{
    public static NamespacedKey<CRWeatherEffectInfo> ToNamespacedKey(this WeatherEffect weatherEffect)
    {
        if (!weatherEffect.TryGetCRInfo(out CRWeatherEffectInfo? weatherEffectInfo))
        {
            Debuggers.Weathers?.Log($"WeatherEffect {weatherEffect} has no CRInfo");
            throw new Exception();
        }
        return weatherEffectInfo.TypedKey;
    }

    internal static bool TryGetCRInfo(this WeatherEffect weatherEffect, [NotNullWhen(true)] out CRWeatherEffectInfo? weatherEffectInfo)
    {
        weatherEffectInfo = (CRWeatherEffectInfo)((ICRObject)weatherEffect).CRInfo;
        return weatherEffectInfo != null;
    }

    internal static void SetCRInfo(this WeatherEffect weatherEffect, CRWeatherEffectInfo weatherEffectInfo)
    {
        ((ICRObject)weatherEffect).CRInfo = weatherEffectInfo;
    }
}
