using System;
using System.Collections.Generic;
using System.Linq;
using Dawn.Internal;
using Dawn.Utils;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Unity.Netcode;
using UnityEngine;

namespace Dawn;

static class WeatherRegistrationHandler
{
    internal static void Init()
    {
        if (WeatherRegistryCompat.Enabled)
        {
            On.Terminal.Start += RegisterVanillaAndModdedWeathers;
        }
        else
        {
            On.TimeOfDay.Awake += RegisterDawnWeathers;
            LethalContent.Moons.AfterTaggingWithContext += _ => AddDawnWeathersToMoons();
            On.StartOfRound.Start += RegisterVanillaAndModdedWeathers;
        }

        On.GameNetcodeStuff.PlayerControllerB.ConnectClientToPlayerObject += SyncWeathers;
        IL.StartOfRound.SetPlanetsWeather += ModifyWeatherWeighting;

        DawnPlugin.Hooks.Add(new Hook(AccessTools.DeclaredMethod(typeof(Enum), nameof(Enum.ToString), Type.EmptyTypes), ProvideDawnWeatherNames));
    }

    private static void RegisterVanillaAndModdedWeathers(On.Terminal.orig_Start orig, Terminal self)
    {
        orig(self);
        AddWeathersToRegistry(); // This should HOPEFULLY run after LethalContent.Moons has collected all the moons
        if (LethalContent.Weathers.IsFrozen)
        {
            return;
        }

        LethalContent.Weathers.Freeze();
        InsertDawnWeathers(TimeOfDayRefs.Instance);
        AddDawnWeathersToMoons();
    }

    private static void AddDawnWeathersToMoons()
    {
        foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
        {
            if (!moonInfo.HasTag(Tags.SupportsWeather))
                continue;

            List<RandomWeatherWithVariables> randomWeathersWithVariables = moonInfo.Level.randomWeathers.ToList();
            foreach (DawnWeatherEffectInfo weatherEffectInfo in LethalContent.Weathers.Values)
            {
                if (weatherEffectInfo.ShouldSkipIgnoreOverride())
                    continue;

                SpawnWeightContext ctx = new SpawnWeightContext(
                    moonInfo,
                    null,
                    LethalContent.Weathers[WeatherKeys.None])
                    .WithExtra(SpawnWeightExtraKeys.RoutingPriceKey, moonInfo.DawnPurchaseInfo.Cost.Provide());

                RandomWeatherWithVariables randomWeatherWithVariables = new RandomWeatherWithVariables()
                {
                    weatherType = weatherEffectInfo.GetLevelWeatherEffect(),
                    weatherVariable = 0,
                    weatherVariable2 = 0
                };

                randomWeathersWithVariables.Add(randomWeatherWithVariables);
            }
            moonInfo.Level.randomWeathers = randomWeathersWithVariables.ToArray();
        }
    }

    private static void ModifyWeatherWeighting(ILContext il)
    {
        ILCursor cursor = new(il);
        if (!cursor.TryGotoNext(
            MoveType.Before,
            il => il.MatchLdloc(5),
            il => il.MatchLdfld<SelectableLevel>(nameof(SelectableLevel.randomWeathers)),
            il => il.MatchLdloc(0),
            il => il.MatchLdcI4(0),
            il => il.MatchLdloc(5),
            il => il.MatchLdfld<SelectableLevel>(nameof(SelectableLevel.randomWeathers)),
            il => il.MatchLdlen(),
            il => il.MatchConvI4(),
            il => il.MatchCallvirt<System.Random>(nameof(System.Random.Next)),
            il => il.MatchLdelemRef(),
            il => il.MatchLdfld<RandomWeatherWithVariables>(nameof(RandomWeatherWithVariables.weatherType)),
            il => il.MatchStfld<SelectableLevel>(nameof(SelectableLevel.currentWeather))
        ))
        {
            DawnPlugin.Logger.LogError($"Failed to find IL for StartOfRound.SetPlanetsWeather (0).");
            return;
        }

        cursor.RemoveRange(11);
        cursor.Emit(OpCodes.Ldloc, 5);
        cursor.Emit(OpCodes.Ldloc, 0);
        cursor.EmitDelegate((SelectableLevel selectableLevel, System.Random random) =>
        {
            List<LevelWeatherType> possibleWeathers = selectableLevel.randomWeathers.Select(x => x.weatherType).ToList();
            List<DawnWeatherEffectInfo> possibleDawnWeathers = new();
            foreach (LevelWeatherType possibleWeather in possibleWeathers)
            {
                if (possibleWeather.DawnInfo != null)
                {
                    possibleDawnWeathers.Add(possibleWeather.DawnInfo);
                }
            }
            DawnWeatherEffectInfo selectedDawnWeather = random.NextItem(possibleDawnWeathers);
            return selectedDawnWeather.GetLevelWeatherEffect();
        });
    }

    private static string ProvideDawnWeatherNames(RuntimeILReferenceBag.FastDelegateInvokers.Func<Enum, string> orig, Enum self)
    {
        if (self.GetType() == typeof(LevelWeatherType))
        {
            int value = (int)(LevelWeatherType)self;
            if (value >= TimeOfDayRefs.Instance.effects.Length || value < 0)
            {
                return orig(self);
            }

            WeatherEffect weatherEffect = TimeOfDayRefs.Instance.effects[value];
            DawnWeatherEffectInfo? weatherEffectInfo = weatherEffect.DawnInfo;
            if (weatherEffectInfo == null || weatherEffectInfo.ShouldSkipIgnoreOverride())
            {
                return orig(self);
            }

            return weatherEffectInfo.WeatherEffect.name;
        }

        return orig(self);
    }

    private static void RegisterDawnWeathers(On.TimeOfDay.orig_Awake orig, TimeOfDay self)
    {
        InsertDawnWeathers(self);
        orig(self);
    }

    private static void InsertDawnWeathers(TimeOfDay self)
    {
        List<WeatherEffect> effectsToSet = self.effects.ToList();
        foreach (DawnWeatherEffectInfo weatherInfo in LethalContent.Weathers.Values)
        {
            if (weatherInfo.ShouldSkipIgnoreOverride() || weatherInfo.WeatherEffect == null)
                continue;

            effectsToSet.Add(weatherInfo.WeatherEffect);
            if (weatherInfo.EffectObjectPrefab != null)
            {
                GameObject newEffectObject = GameObject.Instantiate(weatherInfo.EffectObjectPrefab, self.transform);
                newEffectObject.SetActive(false);
                weatherInfo.WeatherEffect.effectObject = newEffectObject;
            }

            if (weatherInfo.EffectPermanentObjectPrefab != null)
            {
                GameObject newEffectPermanentObject = GameObject.Instantiate(weatherInfo.EffectPermanentObjectPrefab, self.transform);
                newEffectPermanentObject.SetActive(false);
                weatherInfo.WeatherEffect.effectPermanentObject = newEffectPermanentObject;
            }
        }

        self.effects = effectsToSet.ToArray();
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

    private static void RegisterVanillaAndModdedWeathers(On.StartOfRound.orig_Start orig, StartOfRound self)
    {
        AddWeathersToRegistry(); // This should HOPEFULLY run after LethalContent.Moons has collected all the moons
        if (LethalContent.Weathers.IsFrozen)
        {
            orig(self);
            return;
        }

        LethalContent.Weathers.Freeze();
        orig(self);
    }

    private static void AddWeathersToRegistry()
    {
        if (!LethalContent.Weathers.IsFrozen)
        {
            string noneName = NamespacedKey.NormalizeStringForNamespacedKey("none", true);
            NamespacedKey<DawnWeatherEffectInfo> noneKey = NamespacedKey.Vanilla(noneName).AsTyped<DawnWeatherEffectInfo>();
            ProviderTable<int?, DawnMoonInfo, SpawnWeightContext> noneWeights = new WeightTableBuilder<DawnMoonInfo, SpawnWeightContext>().SetGlobalWeight(100).Build();
            // TODO: ugh none isn't even a weather technically, should this really have a weight at all? should I ignore zeekerss' way of getting none weight via the curve? should i try to replicate the curve logic but through pure numerical weights???
            DawnWeatherEffectInfo noneEffectInfo = new(noneKey, [DawnLibTags.IsExternal], null!, noneWeights, 1f, null);
            LethalContent.Weathers.Register(noneEffectInfo);
        }

        foreach ((int i, WeatherEffect weatherEffect) in TimeOfDayRefs.Instance.effects.WithIndex())
        {
            if (weatherEffect.DawnInfo != null)
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
                weatherEffect.DawnInfo = LethalContent.Weathers[key];
                continue;
            }

            // TODO: Grab each weather's weights on the moons, their weather to weather weights too and their base weight since WR supports that
            WeightTableBuilder<DawnMoonInfo, SpawnWeightContext> weatherWeights = new WeightTableBuilder<DawnMoonInfo, SpawnWeightContext>();
            foreach (DawnMoonInfo moonInfo in LethalContent.Moons.Values)
            {
                foreach (RandomWeatherWithVariables randomWeatherWithVariables in moonInfo.Level.randomWeathers)
                {
                    if ((int)randomWeatherWithVariables.weatherType == i)
                    {
                        weatherWeights.AddWeight(moonInfo.TypedKey, 100);
                    }
                }
            }
            DawnWeatherEffectInfo weatherEffectInfo = new(key, [DawnLibTags.IsExternal], weatherEffect, weatherWeights.Build(), 1f, null);
            LethalContent.Weathers.Register(weatherEffectInfo);
            weatherEffect.DawnInfo = weatherEffectInfo;
        }
    }
}
