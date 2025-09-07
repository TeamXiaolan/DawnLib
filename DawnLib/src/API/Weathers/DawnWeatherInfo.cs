using System.Collections.Generic;

namespace Dawn;

public class DawnWeatherEffectInfo : DawnBaseInfo<DawnWeatherEffectInfo>
{
    internal DawnWeatherEffectInfo(NamespacedKey<DawnWeatherEffectInfo> key, List<NamespacedKey> tags, WeatherEffect weatherEffect, DataContainer? customData) : base(key, tags, customData)
    {
        WeatherEffect = weatherEffect;
    }

    public WeatherEffect WeatherEffect { get; }
}