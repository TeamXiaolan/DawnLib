using System;

namespace Dawn;

public static class LevelWeatherTypeExtensions
{
    internal static DawnWeatherEffectInfo GetDawnInfo(this LevelWeatherType levelWeatherType)
    {
        WeatherEffect? weatherEffect = GetWeatherEffect(levelWeatherType);
        if (weatherEffect == null)
        {
            return LethalContent.Weathers[WeatherKeys.None];
        }

        if (!weatherEffect.TryGetDawnInfo(out DawnWeatherEffectInfo? weatherEffectInfo))
        {
            throw new ArgumentException($"WeatherEffect {weatherEffect.name} does not have a DawnWeatherEffectInfo.", nameof(levelWeatherType));
        }
        return weatherEffectInfo;
    }

    internal static bool TryGetDawnInfo(this LevelWeatherType levelWeatherType, out DawnWeatherEffectInfo weatherEffectInfo)
    {
        weatherEffectInfo = levelWeatherType.GetDawnInfo();
        return weatherEffectInfo != null;
    }

    internal static bool HasDawnInfo(this LevelWeatherType levelWeatherType)
    {
        return levelWeatherType.GetDawnInfo() != null;
    }

    public static WeatherEffect? GetWeatherEffect(this LevelWeatherType levelWeatherType)
    {
        if (levelWeatherType == LevelWeatherType.None)
        {
            return null;
        }

        foreach (DawnWeatherEffectInfo weatherEffectInfo in LethalContent.Weathers.Values)
        {
            if (weatherEffectInfo.GetLevelWeatherEffect() == levelWeatherType)
            {
                return weatherEffectInfo.WeatherEffect;
            }
        }

        throw new ArgumentException($"LevelWeatherType {levelWeatherType.ToString()} does not have a DawnWeatherEffectInfo.", nameof(levelWeatherType));
    }
}
