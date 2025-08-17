using System;

namespace CodeRebirthLib;

public class WeatherInfoBuilder
{
    private NamespacedKey<CRWeatherEffectInfo> _key;
    private WeatherEffect _weatherEffect;

    private ProviderTable<int?, CRMoonInfo>? _outsideWeights;

    internal WeatherInfoBuilder(NamespacedKey<CRWeatherEffectInfo> key, WeatherEffect weatherEffect)
    {
        _key = key;
        _weatherEffect = weatherEffect;
    }

    internal CRWeatherEffectInfo Build()
    {
        return new CRWeatherEffectInfo(_key, false, _weatherEffect);
    }
}