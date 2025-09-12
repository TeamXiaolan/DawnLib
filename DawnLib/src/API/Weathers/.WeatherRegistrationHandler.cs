using Dawn.Internal;

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
            if (weatherEffect.HasDawnInfo())
                continue;

            string name = NamespacedKey.NormalizeStringForNamespacedKey(weatherEffect.name, true);
            NamespacedKey<DawnWeatherEffectInfo>? key = WeatherKeys.GetByReflection(name);
            if (key == null && LethalLibCompat.Enabled && LethalLibCompat.TryGetWeatherEffectFromLethalLib(weatherEffect.name, out string lethalLibModName))
            {
                key = NamespacedKey<DawnWeatherEffectInfo>.From(NamespacedKey.NormalizeStringForNamespacedKey(lethalLibModName, false), NamespacedKey.NormalizeStringForNamespacedKey(weatherEffect.name, false));
            }
            else if (key == null && WeatherRegistryCompat.Enabled && WeatherRegistryCompat.TryGetWeatherFromWeatherRegistry(weatherEffect.name, out string weatherRegistryModName))
            {
                key = NamespacedKey<DawnWeatherEffectInfo>.From(NamespacedKey.NormalizeStringForNamespacedKey(weatherRegistryModName, false), NamespacedKey.NormalizeStringForNamespacedKey(weatherEffect.name, false));
            }
            else if (key == null)
            {
                key = NamespacedKey<DawnWeatherEffectInfo>.From("unknown_lib", NamespacedKey.NormalizeStringForNamespacedKey(weatherEffect.name, false));
            }

            if (LethalContent.Weathers.ContainsKey(key))
            {
                DawnPlugin.Logger.LogWarning($"Weather {weatherEffect.name} is already registered by the same creator to LethalContent. This is likely to cause issues.");
                weatherEffect.SetDawnInfo(LethalContent.Weathers[key]);
                continue;
            }

            DawnWeatherEffectInfo weatherEffectInfo = new(key, [DawnLibTags.IsExternal], weatherEffect, null);
            LethalContent.Weathers.Register(weatherEffectInfo);
        }
        LethalContent.Weathers.Freeze();
    }
}