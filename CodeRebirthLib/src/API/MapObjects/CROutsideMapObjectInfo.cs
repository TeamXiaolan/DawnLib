using UnityEngine;

namespace CodeRebirthLib;

public sealed class CROutsideMapObjectInfo
{
    public CRMapObjectInfo ParentInfo { get; internal set; }

    internal CROutsideMapObjectInfo(Table<AnimationCurve?, CRMoonInfo> spawnWeights, bool alignWithTerrain)
    {
        SpawnWeights = spawnWeights;
        AlignWithTerrain = alignWithTerrain;
    }

    public Table<AnimationCurve?, CRMoonInfo> SpawnWeights { get; }
    public bool AlignWithTerrain { get; }
}