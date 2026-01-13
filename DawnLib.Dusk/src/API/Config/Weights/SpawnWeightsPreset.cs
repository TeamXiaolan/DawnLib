using System.Collections.Generic;
using System.Linq;
using Dawn;
using Dawn.Internal;
using Dusk.Weights.Transformers;
using UnityEngine;

namespace Dusk.Weights;

public readonly struct SpawnWeightContext(DawnMoonInfo moon, DawnDungeonInfo dungeon, DawnWeatherEffectInfo? weather)
{
    public readonly DawnMoonInfo Moon = moon;
    public readonly DawnDungeonInfo Dungeon = dungeon;
    public readonly DawnWeatherEffectInfo? Weather = weather;
}

public class SpawnWeightsPreset : IWeighted
{
    public MoonWeightTransformer MoonSpawnWeightsTransformer { get; private set; }
    public InteriorWeightTransformer InteriorSpawnWeightsTransformer { get; private set; }
    public WeatherWeightTransformer WeatherSpawnWeightsTransformer { get; private set; }

    private int _baseWeightIncrease = 0;

    public void SetupSpawnWeightsPreset(List<NamespacedConfigWeight> moonConfig, List<NamespacedConfigWeight> interiorConfig, List<NamespacedConfigWeight> weatherConfig, int baseWeightIncrease = 0)
    {
        MoonSpawnWeightsTransformer = new MoonWeightTransformer(moonConfig);
        InteriorSpawnWeightsTransformer = new InteriorWeightTransformer(interiorConfig);
        WeatherSpawnWeightsTransformer = new WeatherWeightTransformer(weatherConfig);
        _baseWeightIncrease = baseWeightIncrease;
    }

    public int GetWeight(SpawnWeightContext ctx)
    {
        float weight = 0;

        var transformers = new List<(int priority, System.Func<float, float> apply)>()
        {
            (Priority(MoonSpawnWeightsTransformer.GetOperation(ctx.Moon)), currentWeight => MoonSpawnWeightsTransformer.GetNewWeight(currentWeight, ctx.Moon)),

            (Priority(InteriorSpawnWeightsTransformer.GetOperation(ctx.Dungeon)), currentWeight => InteriorSpawnWeightsTransformer.GetNewWeight(currentWeight, ctx.Dungeon)),

            (Priority(WeatherSpawnWeightsTransformer.GetOperation(ctx.Weather)), currentWeight => WeatherSpawnWeightsTransformer.GetNewWeight(currentWeight, ctx.Weather)),
        }
        .OrderByDescending(x => x.priority)
        .ToList();

        foreach (var (priority, apply) in transformers)
        {
            Debuggers.Weights?.Log($"Old Weight: {weight}");
            weight = apply(weight);
            Debuggers.Weights?.Log($"New Weight: {weight}");
        }

        return Mathf.RoundToInt(weight + _baseWeightIncrease);
    }

    public int GetWeight()
    {
        SpawnWeightContext ctx = SpawnWeightContextFactory.FromCurrentGame();
        if (ctx.Moon == null && ctx.Dungeon == null && ctx.Weather == null)
        {
            return 0;
        }
        return GetWeight(ctx);
    }

    private static int Priority(MathOperation operation) => (operation == MathOperation.Additive || operation == MathOperation.Subtractive) ? 1 : 0;
}

public static class SpawnWeightContextFactory
{
    public static SpawnWeightContext FromCurrentGame()
    {
        if (!RoundManager.Instance || RoundManager.Instance.currentLevel == null || RoundManager.Instance.dungeonGenerator == null || RoundManager.Instance.dungeonGenerator.Generator == null || RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow == null || TimeOfDay.Instance == null || TimeOfDay.Instance.currentLevel == null)
        {
            return new SpawnWeightContext(null, null, null);
        }

        DawnMoonInfo moon = RoundManager.Instance.currentLevel.GetDawnInfo();

        DawnDungeonInfo dungeon = RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.GetDawnInfo();

        DawnWeatherEffectInfo? weather = null;
        if (TimeOfDay.Instance.currentLevel.currentWeather != LevelWeatherType.None)
        {
            weather = TimeOfDay.Instance.effects[(int)TimeOfDay.Instance.currentLevel.currentWeather].GetDawnInfo();
        }

        return new SpawnWeightContext(moon, dungeon, weather);
    }
}
