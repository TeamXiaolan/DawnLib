using UnityEngine;

namespace Dawn;

public sealed class DawnOutsideMapObjectInfo
{
    public DawnMapObjectInfo ParentInfo { get; internal set; }

    internal DawnOutsideMapObjectInfo(ProviderTable<AnimationCurve?, DawnMoonInfo> spawnWeights, bool alignWithTerrain)
    {
        SpawnWeights = spawnWeights;
        AlignWithTerrain = alignWithTerrain;
    }

    public ProviderTable<AnimationCurve?, DawnMoonInfo> SpawnWeights { get; }
    public bool AlignWithTerrain { get; }
}