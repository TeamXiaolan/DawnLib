using System;
using Dawn.Interfaces;

namespace Dawn;

public static class WeatherEffectExtensions
{
    internal static DawnWeatherEffectInfo GetDawnInfo(this WeatherEffect weatherEffect)
    {
        DawnWeatherEffectInfo weatherEffectInfo = (DawnWeatherEffectInfo)((IDawnObject)weatherEffect).DawnInfo;
        return weatherEffectInfo;
    }

    internal static bool TryGetDawnInfo(this WeatherEffect weatherEffect, out DawnWeatherEffectInfo weatherEffectInfo)
    {
        weatherEffectInfo = weatherEffect.GetDawnInfo();
        return weatherEffectInfo != null;
    }

    internal static bool HasDawnInfo(this WeatherEffect weatherEffect)
    {
        return weatherEffect.GetDawnInfo() != null;
    }

    internal static void SetDawnInfo(this WeatherEffect weatherEffect, DawnWeatherEffectInfo weatherEffectInfo)
    {
        ((IDawnObject)weatherEffect).DawnInfo = weatherEffectInfo;
    }

    public static LevelWeatherType GetLevelWeatherType(this WeatherEffect weatherEffect)
    {
        if (!weatherEffect.TryGetDawnInfo(out DawnWeatherEffectInfo weatherEffectInfo))
        {
            throw new ArgumentException($"WeatherEffect {weatherEffect.name} does not have a DawnWeatherEffectInfo.", nameof(weatherEffect));
        }

        return weatherEffectInfo.GetLevelWeatherEffect();
    }
}
