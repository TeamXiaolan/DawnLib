using System;
using Dawn.Interfaces;

namespace Dawn;

public static class WeatherEffectExtensions
{
    extension(WeatherEffect weatherEffect)
    {
        public DawnWeatherEffectInfo DawnInfo
        {
            get => weatherEffect.GetDawnInfoCore();
            set => weatherEffect.SetDawnInfoCore(value);
        }

        [Obsolete("Use WeatherEffect.DawnInfo instead")]
        public DawnWeatherEffectInfo GetDawnInfo()
        {
            return weatherEffect.GetDawnInfoCore();
        }

        [Obsolete("Use WeatherEffect.DawnInfo instead")]
        public void SetDawnInfo(DawnWeatherEffectInfo weatherEffectInfo)
        {
            weatherEffect.SetDawnInfoCore(weatherEffectInfo);
        }

        private DawnWeatherEffectInfo GetDawnInfoCore()
        {
            return ((IWeatherEffectDawnObject)weatherEffect).DawnInfo;
        }

        private void SetDawnInfoCore(DawnWeatherEffectInfo weatherEffectInfo)
        {
            ((IWeatherEffectDawnObject)weatherEffect).DawnInfo = weatherEffectInfo;
        }

        public LevelWeatherType GetLevelWeatherType()
        {
            if (weatherEffect.DawnInfo == null)
            {
                throw new ArgumentException($"WeatherEffect {weatherEffect.name} does not have a DawnWeatherEffectInfo.", nameof(weatherEffect));
            }

            return weatherEffect.DawnInfo.GetLevelWeatherEffect();
        }
    }
}