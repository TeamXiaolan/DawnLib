using System.Collections.Generic;

namespace Dawn;

public class DawnWeatherEffectInfo : DawnBaseInfo<DawnWeatherEffectInfo>
{
    internal DawnWeatherEffectInfo(NamespacedKey<DawnWeatherEffectInfo> key, HashSet<NamespacedKey> tags, WeatherEffect weatherEffect, IDataContainer? customData) : base(key, tags, customData)
    {
        WeatherEffect = weatherEffect;
    }

    public WeatherEffect WeatherEffect { get; }
}