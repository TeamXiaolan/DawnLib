using System;
using System.Collections.Generic;
using Dawn.Internal;
using Dawn.Utils;
using UnityEngine;

namespace Dawn;

public class DawnWeatherEffectInfo : DawnBaseInfo<DawnWeatherEffectInfo>
{
    internal DawnWeatherEffectInfo(NamespacedKey<DawnWeatherEffectInfo> key, HashSet<NamespacedKey> tags, WeatherEffect weatherEffect, ProviderTable<int?, DawnMoonInfo, SpawnWeightContext> weights, float lerpSpeed, IDataContainer? customData) : base(key, tags, customData)
    {
        WeatherEffect = weatherEffect;

        if (weatherEffect != null)
        {
            EffectObjectPrefab = weatherEffect.effectObject;
            EffectPermanentObjectPrefab = weatherEffect.effectPermanentObject;
        }

        Weights = weights;
        LerpSpeed = lerpSpeed;
    }

    public WeatherEffect WeatherEffect { get; internal set; } // Only null for None weather

    public GameObject? EffectObjectPrefab { get; }
    public GameObject? EffectPermanentObjectPrefab { get; }

    public ProviderTable<int?, DawnMoonInfo, SpawnWeightContext> Weights { get; }
    public float LerpSpeed { get; } // Change values like this to be preloaded in?

    public LevelWeatherType GetLevelWeatherEffect()
    {
        if (WeatherEffect == null)
        {
            return LevelWeatherType.None;
        }

        foreach ((int i, WeatherEffect potentialWeatherEffect) in TimeOfDayRefs.Instance.effects.WithIndex())
        {
            if (potentialWeatherEffect == WeatherEffect)
            {
                return (LevelWeatherType)i;
            }
        }

        throw new ArgumentException($"WeatherEffect {WeatherEffect.name} does not have a LevelWeatherType.", nameof(WeatherEffect));
    }
}