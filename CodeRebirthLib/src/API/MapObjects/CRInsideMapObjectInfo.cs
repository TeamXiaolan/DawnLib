using UnityEngine;

namespace CodeRebirthLib;

public sealed class CRInsideMapObjectInfo
{
    public CRMapObjectInfo ParentInfo { get; internal set; }

    internal CRInsideMapObjectInfo(ProviderTable<AnimationCurve?, CRMoonInfo> spawnWeights, bool spawnFacingAwayFromWall, bool spawnFacingWall, bool spawnWithBackToWall, bool spawnWithBackFlushAgainstWall, bool requireDistanceBetweenSpawns, bool disallowSpawningNearEntrances)
    {
        SpawnWeights = spawnWeights;
        SpawnFacingAwayFromWall = spawnFacingAwayFromWall;
        SpawnFacingWall = spawnFacingWall;
        SpawnWithBackToWall = spawnWithBackToWall;
        SpawnWithBackFlushAgainstWall = spawnWithBackFlushAgainstWall;
        RequireDistanceBetweenSpawns = requireDistanceBetweenSpawns;
        DisallowSpawningNearEntrances = disallowSpawningNearEntrances;
    }

    public ProviderTable<AnimationCurve?, CRMoonInfo> SpawnWeights { get; }
    public bool SpawnFacingAwayFromWall { get; }
    public bool SpawnFacingWall { get; }
    public bool SpawnWithBackToWall { get; }
    public bool SpawnWithBackFlushAgainstWall { get; }
    public bool RequireDistanceBetweenSpawns { get; }
    public bool DisallowSpawningNearEntrances { get; }
}