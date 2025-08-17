using System.Reflection;

namespace CodeRebirthLib;

public static class WeatherEffectExtensions
{
    // todo: reference stripped patched assembly??
    private static FieldInfo _infoField = typeof(WeatherEffect).GetField("__crinfo", BindingFlags.Instance | BindingFlags.Public);

    public static NamespacedKey<CRWeatherEffectInfo>? ToNamespacedKey(this WeatherEffect weatherEffect)
    {
        if (!weatherEffect.HasCRInfo())
        {
            CodeRebirthLibPlugin.Logger.LogError($"WeatherEffect '{weatherEffect.name}' does not have a CRWeatherInfo, you are either accessing this too early or it erroneously never got created!");
            return null;
        }
        return weatherEffect.GetCRInfo().TypedKey;
    }

    internal static bool HasCRInfo(this WeatherEffect weatherEffect)
    {
        return _infoField.GetValue(weatherEffect) != null;
    }

    internal static CRWeatherEffectInfo GetCRInfo(this WeatherEffect weatherEffect)
    {
        return (CRWeatherEffectInfo)_infoField.GetValue(weatherEffect);
    }

    internal static void SetCRInfo(this WeatherEffect weatherEffect, CRWeatherEffectInfo info)
    {
        _infoField.SetValue(weatherEffect, info);
    }
}
