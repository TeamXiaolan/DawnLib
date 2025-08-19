namespace CodeRebirthLib;

public class CRWeatherEffectInfo : CRBaseInfo<CRWeatherEffectInfo>
{
    internal CRWeatherEffectInfo(NamespacedKey<CRWeatherEffectInfo> key, bool isExternal, WeatherEffect weatherEffect) : base(key, [CRLibTags.IsExternal]) // todo
    {
        WeatherEffect = weatherEffect;
    }

    public WeatherEffect WeatherEffect { get; }
}