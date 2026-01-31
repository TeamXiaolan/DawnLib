using Dawn.Internal;

namespace Dawn;

static class WeatherRegistrationHandler
{
    internal static void Init()
    {
        On.Terminal.Awake += RegisterVanillaWeathers;
        On.Terminal.Start += RegisterModdedWeathers;
    }

    private static void RegisterVanillaWeathers(On.Terminal.orig_Awake orig, Terminal self)
    {
        AddWeathersToRegistry();
        orig(self);
    }

    private static void RegisterModdedWeathers(On.Terminal.orig_Start orig, Terminal self)
    {
        orig(self);
        AddWeathersToRegistry();
        LethalContent.Weathers.Freeze();
    }

    private static void AddWeathersToRegistry()
    {
        foreach (WeatherEffect weatherEffect in TimeOfDayRefs.Instance.effects)
        {
            if (weatherEffect.HasDawnInfo())
                continue;

            string name = NamespacedKey.NormalizeStringForNamespacedKey(weatherEffect.name, true);
            NamespacedKey<DawnWeatherEffectInfo>? key = WeatherKeys.GetByReflection(name);
            if (key == null && LethalLibCompat.Enabled && LethalLibCompat.TryGetWeatherEffectFromLethalLib(weatherEffect.name, out string lethalLibModName))
            {
                key = NamespacedKey<DawnWeatherEffectInfo>.From(lethalLibModName, weatherEffect.name);
            }
            else if (key == null && WeatherRegistryCompat.Enabled && WeatherRegistryCompat.TryGetWeatherFromWeatherRegistry(weatherEffect.name, out string weatherRegistryModName))
            {
                key = NamespacedKey<DawnWeatherEffectInfo>.From(weatherRegistryModName, weatherEffect.name);
            }
            else if (key == null)
            {
                key = NamespacedKey<DawnWeatherEffectInfo>.From("unknown_lib", weatherEffect.name);
            }

            if (LethalContent.Weathers.ContainsKey(key))
            {
                DawnPlugin.Logger.LogWarning($"Weather {weatherEffect.name} is already registered by the same creator to LethalContent. This is likely to cause issues unless caused by lobby reloads.");
                LethalContent.Weathers[key].WeatherEffect = weatherEffect;
                weatherEffect.SetDawnInfo(LethalContent.Weathers[key]);
                continue;
            }

            DawnWeatherEffectInfo weatherEffectInfo = new(key, [DawnLibTags.IsExternal], weatherEffect, null);
            LethalContent.Weathers.Register(weatherEffectInfo);
            weatherEffect.SetDawnInfo(weatherEffectInfo);
        }
    }
}
