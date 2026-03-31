using UnityEngine;

namespace Dawn;

public sealed class DawnOutsideMapObjectInfo
{
    public DawnMapObjectInfo ParentInfo { get; internal set; }

    internal DawnOutsideMapObjectInfo(SpawnableOutsideObject spawnableOutsideObject, ProviderTable<AnimationCurve?, DawnMoonInfo, SpawnWeightContext> spawnWeights, bool alignWithTerrain, int minimumAINodeSpawnRequirement)
    {
        SpawnableOutsideObject = spawnableOutsideObject;
        SpawnWeights = spawnWeights;
        AlignWithTerrain = alignWithTerrain;
        MinimumAINodeSpawnRequirement = minimumAINodeSpawnRequirement;
    }

    public SpawnableOutsideObject SpawnableOutsideObject { get; private set; }
    public ProviderTable<AnimationCurve?, DawnMoonInfo, SpawnWeightContext> SpawnWeights { get; private set; }
    public bool AlignWithTerrain { get; private set; }
    public int MinimumAINodeSpawnRequirement { get; private set; }
}