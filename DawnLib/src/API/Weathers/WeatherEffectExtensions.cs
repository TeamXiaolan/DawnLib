using System;
using System.Diagnostics.CodeAnalysis;
using Dawn.Internal;
using Dawn.Preloader.Interfaces;

namespace Dawn;

public static class WeatherEffectExtensions
{
    public static NamespacedKey<DawnWeatherEffectInfo> ToNamespacedKey(this WeatherEffect weatherEffect)
    {
        if (!weatherEffect.TryGetCRInfo(out DawnWeatherEffectInfo? weatherEffectInfo))
        {
            Debuggers.Weathers?.Log($"WeatherEffect {weatherEffect} has no CRInfo");
            throw new Exception();
        }
        return weatherEffectInfo.TypedKey;
    }

    internal static bool TryGetCRInfo(this WeatherEffect weatherEffect, [NotNullWhen(true)] out DawnWeatherEffectInfo? weatherEffectInfo)
    {
        weatherEffectInfo = (DawnWeatherEffectInfo)((ICRObject)weatherEffect).CRInfo;
        return weatherEffectInfo != null;
    }

    internal static void SetCRInfo(this WeatherEffect weatherEffect, DawnWeatherEffectInfo weatherEffectInfo)
    {
        ((ICRObject)weatherEffect).CRInfo = weatherEffectInfo;
    }
}
