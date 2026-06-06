using System;

namespace Dawn;

public static class LevelWeatherTypeExtensions
{
    extension(LevelWeatherType levelWeatherType)
    {
        public DawnWeatherEffectInfo DawnInfo
        {
            get => levelWeatherType.GetDawnInfoCore();
        }

        [Obsolete("Use LevelWeatherType.DawnInfo instead")]
        public DawnWeatherEffectInfo GetDawnInfo()
        {
            return levelWeatherType.GetDawnInfoCore();
        }

        private DawnWeatherEffectInfo GetDawnInfoCore()
        {
            WeatherEffect? weatherEffect = GetWeatherEffect(levelWeatherType);
            if (weatherEffect == null)
            {
                return LethalContent.Weathers[WeatherKeys.None];
            }

            if (weatherEffect.DawnInfo == null)
            {
                throw new ArgumentException($"WeatherEffect {weatherEffect.name} does not have a DawnWeatherEffectInfo.", nameof(levelWeatherType));
            }
            return weatherEffect.DawnInfo;
        }

        public WeatherEffect? GetWeatherEffect()
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
}
