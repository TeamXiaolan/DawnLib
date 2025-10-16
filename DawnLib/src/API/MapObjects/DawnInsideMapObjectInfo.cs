using UnityEngine;

namespace Dawn;

public sealed class DawnInsideMapObjectInfo
{
    public DawnMapObjectInfo ParentInfo { get; internal set; }

    internal DawnInsideMapObjectInfo(ProviderTable<AnimationCurve?, DawnMoonInfo> spawnWeights, bool spawnFacingAwayFromWall, bool spawnFacingWall, bool spawnWithBackToWall, bool spawnWithBackFlushAgainstWall, bool requireDistanceBetweenSpawns, bool disallowSpawningNearEntrances)
    {
        SpawnWeights = spawnWeights;
        SpawnFacingAwayFromWall = spawnFacingAwayFromWall;
        SpawnFacingWall = spawnFacingWall;
        SpawnWithBackToWall = spawnWithBackToWall;
        SpawnWithBackFlushAgainstWall = spawnWithBackFlushAgainstWall;
        RequireDistanceBetweenSpawns = requireDistanceBetweenSpawns;
        DisallowSpawningNearEntrances = disallowSpawningNearEntrances;
    }

    public ProviderTable<AnimationCurve?, DawnMoonInfo> SpawnWeights { get; private set; }
    public bool SpawnFacingAwayFromWall { get; private set; }
    public bool SpawnFacingWall { get; private set; }
    public bool SpawnWithBackToWall { get; private set; }
    public bool SpawnWithBackFlushAgainstWall { get; private set; }
    public bool RequireDistanceBetweenSpawns { get; private set; }
    public bool DisallowSpawningNearEntrances { get; private set; }
}