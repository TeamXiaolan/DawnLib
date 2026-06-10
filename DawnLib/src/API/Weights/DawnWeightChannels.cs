using UnityEngine;

namespace Dawn;

public static class DawnWeightChannels
{
    public static readonly WeightChannel<int> EnemyRarity =
        WeightChannel<int>.From(DawnKeys.EnemyRarity, IntWeightValuePolicy.ClampZero);

    public static readonly WeightChannel<int> DungeonRarity =
        WeightChannel<int>.From(DawnKeys.DungeonRarity, IntWeightValuePolicy.ClampZero);

    public static readonly WeightChannel<int> ScrapRarity =
        WeightChannel<int>.From(DawnKeys.ScrapRarity, IntWeightValuePolicy.ClampZero);

    public static readonly WeightChannel<int> WeatherRarity =
        WeightChannel<int>.From(DawnKeys.WeatherRarity, IntWeightValuePolicy.ClampZero);

    public static readonly WeightChannel<int> MoonSceneRarity =
        WeightChannel<int>.From(DawnKeys.MoonSceneRarity, IntWeightValuePolicy.ClampZero);

    public static readonly WeightChannel<AnimationCurve?> MapObjectSpawnCurve =
        WeightChannel<AnimationCurve?>.From(DawnKeys.MapObjectSpawnCurve, PassthroughWeightValuePolicy<AnimationCurve?>.Default);
}