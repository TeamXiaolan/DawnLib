namespace CodeRebirthLib;

public class CRWeatherInfo : CRBaseInfo<CRWeatherInfo>
{
    public CRWeatherInfo(NamespacedKey<CRWeatherInfo> key, bool isExternal, WeatherEffect weatherEffect) : base(key, isExternal)
    {
        WeatherEffect = weatherEffect;
    }

    public WeatherEffect WeatherEffect { get; }
}