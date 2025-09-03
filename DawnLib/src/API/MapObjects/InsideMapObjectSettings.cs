using System;

namespace Dawn;

[Serializable]
public class InsideMapObjectSettings
{
    public bool spawnFacingAwayFromWall;
    public bool spawnFacingWall;
    public bool spawnWithBackToWall;
    public bool spawnWithBackFlushAgainstWall;
    public bool requireDistanceBetweenSpawns;
    public bool disallowSpawningNearEntrances;
}