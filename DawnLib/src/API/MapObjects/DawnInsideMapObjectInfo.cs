using UnityEngine;

namespace Dawn;

public sealed class DawnInsideMapObjectInfo
{
    public DawnMapObjectInfo ParentInfo { get; internal set; }

    internal DawnInsideMapObjectInfo(IndoorMapHazardType indoorMapHazardType, DawnWeightedValue<AnimationCurve?> rarity)
    {
        IndoorMapHazardType = indoorMapHazardType;
        Rarity = rarity;
    }

    public IndoorMapHazardType IndoorMapHazardType { get; set; }
    public DawnWeightedValue<AnimationCurve?> Rarity { get; private set; }

    public AnimationCurve? GetRarity(DawnMoonInfo? moonInfo = null, DawnDungeonInfo? dungeonInfo = null, DawnWeatherEffectInfo? weatherEffectInfo = null)
    {
        return Rarity.GetValue(new WeightQuery
        {
            Owner = ParentInfo,
            Subject = this,
            Moon = moonInfo,
            Dungeon = dungeonInfo,
            Weather = weatherEffectInfo,
            Channel = DawnWeightChannels.MapObjectSpawnCurve.Key
        });
    }
}