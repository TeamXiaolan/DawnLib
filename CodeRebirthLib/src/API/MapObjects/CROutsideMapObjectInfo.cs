using UnityEngine;

namespace CodeRebirthLib;

public sealed class CROutsideMapObjectInfo
{
    public CRMapObjectInfo ParentInfo { get; internal set; }

    internal CROutsideMapObjectInfo(ProviderTable<AnimationCurve?, CRMoonInfo> spawnWeights, bool alignWithTerrain)
    {
        SpawnWeights = spawnWeights;
        AlignWithTerrain = alignWithTerrain;
    }

    public ProviderTable<AnimationCurve?, CRMoonInfo> SpawnWeights { get; }
    public bool AlignWithTerrain { get; }
}