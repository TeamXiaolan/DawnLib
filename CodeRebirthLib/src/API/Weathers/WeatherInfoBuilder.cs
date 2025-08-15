using System;

namespace CodeRebirthLib;

public class WeatherInfoBuilder
{
    private NamespacedKey<CRWeatherInfo> _key;
    private WeatherEffect _weatherEffect;

    private ProviderTable<int?, CRMoonInfo>? _outsideWeights;

    internal WeatherInfoBuilder(NamespacedKey<CRWeatherInfo> key, WeatherEffect weatherEffect)
    {
        _key = key;
        _weatherEffect = weatherEffect;
    }

    internal CRWeatherInfo Build()
    {
        return new CRWeatherInfo(_key, false, _weatherEffect);
    }
}