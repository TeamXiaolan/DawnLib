using System;

namespace Dawn;

public class WeatherEffectInfoBuilder : BaseInfoBuilder<DawnWeatherEffectInfo, WeatherEffect, WeatherEffectInfoBuilder>
{

    private float _lerpSpeed = 1f;
    private ProviderTable<int?, DawnMoonInfo, SpawnWeightContext>? _weights = null;

    internal WeatherEffectInfoBuilder(NamespacedKey<DawnWeatherEffectInfo> key, WeatherEffect value) : base(key, value)
    {
    }

    public WeatherEffectInfoBuilder OverrideLerpSpeed(float lerpSpeed)
    {
        _lerpSpeed = lerpSpeed;
        return this;
    }

    public WeatherEffectInfoBuilder SetWeights(Action<WeightTableBuilder<DawnMoonInfo, SpawnWeightContext>> callback)
    {
        WeightTableBuilder<DawnMoonInfo, SpawnWeightContext> builder = new WeightTableBuilder<DawnMoonInfo, SpawnWeightContext>();
        callback(builder);
        _weights = builder.Build();
        return this;
    }

    override internal DawnWeatherEffectInfo Build()
    {
        if (_weights == null)
        {
            DawnPlugin.Logger.LogWarning($"WeatherEffect '{key}' didn't set weights. If you intend to have no weights (doing something special), call .SetWeights(() => {{}})");
            _weights = ProviderTable<int?, DawnMoonInfo, SpawnWeightContext>.Empty();
        }

        return new DawnWeatherEffectInfo(key, tags, value, _weights, _lerpSpeed, customData);
    }
}