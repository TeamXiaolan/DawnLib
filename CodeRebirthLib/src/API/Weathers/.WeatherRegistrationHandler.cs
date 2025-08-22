namespace CodeRebirthLib;

static class WeatherRegistrationHandler
{
    internal static void Init()
    {
        On.Terminal.Start += RegisterWeathers;
    }

    private static void RegisterWeathers(On.Terminal.orig_Start orig, Terminal self)
    {
        orig(self);
        if (LethalContent.Weathers.IsFrozen)
            return;

        foreach (WeatherEffect weatherEffect in TimeOfDay.Instance.effects)
        {
            if (weatherEffect.TryGetCRInfo(out _))
                continue;

            NamespacedKey<CRWeatherEffectInfo>? key = (NamespacedKey<CRWeatherEffectInfo>?)typeof(WeatherKeys).GetField(NamespacedKey.NormalizeStringForNamespacedKey(weatherEffect.name, true))?.GetValue(null);
            key ??= NamespacedKey<CRWeatherEffectInfo>.From("modded_please_replace_this_later", NamespacedKey.NormalizeStringForNamespacedKey(weatherEffect.name, false));
            // TODO something about crlib weathers being registered with this namespace instead of code_rebirth for example

            CRWeatherEffectInfo weatherEffectInfo = new(key, [CRLibTags.IsExternal], weatherEffect);
            LethalContent.Weathers.Register(weatherEffectInfo);
        }
        LethalContent.Weathers.Freeze();
    }
}