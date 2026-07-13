using System;
using System.Collections.Generic;
using System.Linq;
using Dawn.Internal;

namespace Dawn;

public sealed class WeatherListBaseRaritySource : WeightModifierSource<int>
{
    private readonly LevelWeatherType _weatherType;
    private readonly Func<int> _getBaseRarity;

    public WeatherListBaseRaritySource(LevelWeatherType weatherType, Func<int> getBaseRarity)
    {
        _weatherType = weatherType;
        _getBaseRarity = getBaseRarity;
    }

    public override void Build(WeightBuildContext context, List<IWeightModifier<int>> modifiers)
    {
        int baseRarity = _getBaseRarity();
        if (baseRarity <= 0)
            return;

        foreach (DawnMoonInfo moonInfo in context.Moons.Values)
        {
            if (!MoonHasWeather(moonInfo, _weatherType))
                continue;

            Debuggers.Weathers?.Log($"Adding weight {baseRarity} for {_weatherType} on level {moonInfo.Level.PlanetName}");
            modifiers.Add(new MoonBaseIntModifier(moonInfo.TypedKey, baseRarity));
        }
    }

    private static bool MoonHasWeather(DawnMoonInfo moonInfo, LevelWeatherType weatherType)
    {
        SelectableLevel level = moonInfo.Level;

        if (level.randomWeathers == null)
            return false;

        return level.randomWeathers.Any(weather => weather.weatherType == weatherType);
    }
}