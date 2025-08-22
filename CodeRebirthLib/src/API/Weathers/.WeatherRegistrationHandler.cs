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

        foreach (WeatherEffect effect in TimeOfDay.Instance.effects)
        {
            if (effect.HasCRInfo())
                continue;

            NamespacedKey<CRWeatherEffectInfo>? key = (NamespacedKey<CRWeatherEffectInfo>?)typeof(WeatherKeys).GetField(NamespacedKey.NormalizeStringForNamespacedKey(effect.name, true))?.GetValue(null);
            key ??= NamespacedKey<CRWeatherEffectInfo>.From("modded_please_replace_this_later", NamespacedKey.NormalizeStringForNamespacedKey(effect.name, false));
            // TODO something about crlib weathers being registered with this namespace instead of code_rebirth for example

            CRWeatherEffectInfo weatherEffectInfo = new(key, [CRLibTags.IsExternal], effect);
            LethalContent.Weathers.Register(weatherEffectInfo);
        }
        LethalContent.Weathers.Freeze();
    }
}