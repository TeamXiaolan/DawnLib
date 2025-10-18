using Dawn.Interfaces;

namespace Dawn;

public static class WeatherEffectExtensions
{
    internal static DawnWeatherEffectInfo GetDawnInfo(this WeatherEffect weatherEffect)
    {
        DawnWeatherEffectInfo weatherEffectInfo = (DawnWeatherEffectInfo)((IDawnObject)weatherEffect).DawnInfo;
        return weatherEffectInfo;
    }

    internal static bool HasDawnInfo(this WeatherEffect weatherEffect)
    {
        return weatherEffect.GetDawnInfo() != null;
    }

    internal static void SetDawnInfo(this WeatherEffect weatherEffect, DawnWeatherEffectInfo weatherEffectInfo)
    {
        ((IDawnObject)weatherEffect).DawnInfo = weatherEffectInfo;
    }
}
