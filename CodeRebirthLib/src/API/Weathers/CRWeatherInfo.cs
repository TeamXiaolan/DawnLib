using System.Collections.Generic;

namespace CodeRebirthLib;

public class CRWeatherEffectInfo : CRBaseInfo<CRWeatherEffectInfo>
{
    internal CRWeatherEffectInfo(NamespacedKey<CRWeatherEffectInfo> key, List<NamespacedKey> tags, WeatherEffect weatherEffect) : base(key, tags) // todo
    {
        WeatherEffect = weatherEffect;
    }

    public WeatherEffect WeatherEffect { get; }
}