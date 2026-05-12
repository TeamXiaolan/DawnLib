using System;
using Dawn.Internal;
using Dawn.Utils;
using Unity.Netcode;

namespace Dawn;

static class WeatherRegistrationHandler
{
    internal static void Init()
    {
        On.Terminal.Awake += RegisterVanillaWeathers;
        On.Terminal.Start += RegisterModdedWeathers;

        On.GameNetcodeStuff.PlayerControllerB.ConnectClientToPlayerObject += SyncWeathers;
    }

    private static void SyncWeathers(On.GameNetcodeStuff.PlayerControllerB.orig_ConnectClientToPlayerObject orig, GameNetcodeStuff.PlayerControllerB self)
    {
        orig(self);
        if (NetworkManager.Singleton.IsServer || !self.IsLocalPlayer())
        {
            return;
        }

        self.playersManager.SetMapScreenInfoToCurrentLevel();
        LevelWeatherType[] levelWeatherTypes = new LevelWeatherType[LethalContent.Moons.Count];
        foreach ((int i, DawnMoonInfo moonInfo) in LethalContent.Moons.Values.WithIndex())
        {
            levelWeatherTypes[i] = moonInfo.Level.currentWeather;
        }

        DawnNetworker.Instance?.RequestWeatherSyncRpc(levelWeatherTypes, self.actualClientId);
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
        if (LethalContent.Weathers.IsFrozen)
        {
            return;
        }

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
                if (!LethalContent.Weathers.IsFrozen)
                {
                    DawnPlugin.Logger.LogWarning($"Weather {weatherEffect.name} is already registered by the same creator to LethalContent. This is likely to cause issues.");
                }
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
