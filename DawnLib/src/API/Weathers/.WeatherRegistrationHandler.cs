namespace Dawn;

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

            string name = NamespacedKey.NormalizeStringForNamespacedKey(weatherEffect.name, true);
            NamespacedKey<DawnWeatherEffectInfo>? key = WeatherKeys.GetByReflection(name);
            key ??= NamespacedKey<DawnWeatherEffectInfo>.From("modded_please_replace_this_later", NamespacedKey.NormalizeStringForNamespacedKey(weatherEffect.name, false));
            if (LethalContent.Weathers.ContainsKey(key))
            {
                DawnPlugin.Logger.LogWarning($"Weather {weatherEffect.name} is already registered by the same creator to LethalContent. Skipping...");
                continue;
            }

            DawnWeatherEffectInfo weatherEffectInfo = new(key, [DawnLibTags.IsExternal], weatherEffect);
            LethalContent.Weathers.Register(weatherEffectInfo);
        }
        LethalContent.Weathers.Freeze();
    }
}