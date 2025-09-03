using System;
using System.Diagnostics.CodeAnalysis;
using Dawn.Internal;
using Dawn.Preloader.Interfaces;

namespace Dawn;

public static class WeatherEffectExtensions
{
    public static NamespacedKey<DawnWeatherEffectInfo> ToNamespacedKey(this WeatherEffect weatherEffect)
    {
        if (!weatherEffect.TryGetDawnInfo(out DawnWeatherEffectInfo? weatherEffectInfo))
        {
            Debuggers.Weathers?.Log($"WeatherEffect {weatherEffect} has no CRInfo");
            throw new Exception();
        }
        return weatherEffectInfo.TypedKey;
    }

    internal static bool TryGetDawnInfo(this WeatherEffect weatherEffect, [NotNullWhen(true)] out DawnWeatherEffectInfo? weatherEffectInfo)
    {
        weatherEffectInfo = (DawnWeatherEffectInfo)((IDawnObject)weatherEffect).DawnInfo;
        return weatherEffectInfo != null;
    }

    internal static void SetDawnInfo(this WeatherEffect weatherEffect, DawnWeatherEffectInfo weatherEffectInfo)
    {
        ((IDawnObject)weatherEffect).DawnInfo = weatherEffectInfo;
    }
}
