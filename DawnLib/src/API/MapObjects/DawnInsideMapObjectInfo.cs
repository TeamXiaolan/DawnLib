using UnityEngine;

namespace Dawn;

public sealed class DawnInsideMapObjectInfo
{
    public DawnMapObjectInfo ParentInfo { get; internal set; }

    internal DawnInsideMapObjectInfo(IndoorMapHazardType indoorMapHazardType, ProviderTable<AnimationCurve?, DawnMoonInfo, SpawnWeightContext> spawnWeights)
    {
        IndoorMapHazardType = indoorMapHazardType;
        SpawnWeights = spawnWeights;
    }

    public IndoorMapHazardType IndoorMapHazardType { get; set; }
    public ProviderTable<AnimationCurve?, DawnMoonInfo, SpawnWeightContext> SpawnWeights { get; private set; }
}