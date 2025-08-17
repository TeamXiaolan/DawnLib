namespace CodeRebirthLib;

public class CRWeatherEffectInfo : CRBaseInfo<CRWeatherEffectInfo>
{
    public CRWeatherEffectInfo(NamespacedKey<CRWeatherEffectInfo> key, bool isExternal, WeatherEffect weatherEffect) : base(key, isExternal)
    {
        WeatherEffect = weatherEffect;
    }

    public WeatherEffect WeatherEffect { get; }
}