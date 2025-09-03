using System.Collections.Generic;

namespace Dawn;

public class DawnWeatherEffectInfo : DawnBaseInfo<DawnWeatherEffectInfo>
{
    internal DawnWeatherEffectInfo(NamespacedKey<DawnWeatherEffectInfo> key, List<NamespacedKey> tags, WeatherEffect weatherEffect) : base(key, tags)
    {
        WeatherEffect = weatherEffect;
    }

    public WeatherEffect WeatherEffect { get; }
}