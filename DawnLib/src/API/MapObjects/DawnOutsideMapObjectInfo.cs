using UnityEngine;

namespace Dawn;

public sealed class DawnOutsideMapObjectInfo
{
    public DawnMapObjectInfo ParentInfo { get; internal set; }

    internal DawnOutsideMapObjectInfo(SpawnableOutsideObject spawnableOutsideObject, DawnWeightedValue<AnimationCurve?> rarity, bool alignWithTerrain, int minimumAINodeSpawnRequirement)
    {
        SpawnableOutsideObject = spawnableOutsideObject;
        Rarity = rarity;
        AlignWithTerrain = alignWithTerrain;
        MinimumAINodeSpawnRequirement = minimumAINodeSpawnRequirement;
    }

    public SpawnableOutsideObject SpawnableOutsideObject { get; private set; }
    public DawnWeightedValue<AnimationCurve?> Rarity { get; private set; }
    public bool AlignWithTerrain { get; private set; }
    public int MinimumAINodeSpawnRequirement { get; private set; }

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