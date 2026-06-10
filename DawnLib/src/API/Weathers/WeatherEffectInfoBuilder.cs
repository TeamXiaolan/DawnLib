using System;

namespace Dawn;

public class WeatherEffectInfoBuilder : BaseInfoBuilder<DawnWeatherEffectInfo, WeatherEffect, WeatherEffectInfoBuilder>
{

    private float _lerpSpeed = 1f;
    private DawnWeightedValue<int>? _weights = null;

    internal WeatherEffectInfoBuilder(NamespacedKey<DawnWeatherEffectInfo> key, WeatherEffect value) : base(key, value)
    {
    }

    public WeatherEffectInfoBuilder OverrideLerpSpeed(float lerpSpeed)
    {
        _lerpSpeed = lerpSpeed;
        return this;
    }

    public WeatherEffectInfoBuilder SetWeights(Action<WeightProfile<int>> callback)
    {
        WeightProfile<int> weightProfile = new WeightProfile<int>(DawnWeightChannels.WeatherRarity.Policy);
        callback(weightProfile);
        _weights = new DawnWeightedValue<int>(DawnWeightChannels.WeatherRarity, weightProfile);
        return this;
    }

    override internal DawnWeatherEffectInfo Build()
    {
        if (_weights == null)
        {
            DawnPlugin.Logger.LogWarning($"WeatherEffect '{key}' didn't set weights. If you intend to have no weights (doing something special), call .SetWeights(() => {{}})");
            _weights = new DawnWeightedValue<int>(DawnWeightChannels.WeatherRarity);
        }

        return new DawnWeatherEffectInfo(key, tags, value, _weights, _lerpSpeed, customData);
    }
}