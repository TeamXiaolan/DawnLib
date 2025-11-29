using UnityEngine;

namespace Dawn;

public sealed class DawnOutsideMapObjectInfo
{
    public DawnMapObjectInfo ParentInfo { get; internal set; }

    internal DawnOutsideMapObjectInfo(ProviderTable<AnimationCurve?, DawnMoonInfo> spawnWeights, bool spawnFacingAwayFromWall, int objectWidth, string[] spawnableFloorTags, Vector3 rotationOffset, bool alignWithTerrain)
    {
        SpawnWeights = spawnWeights;
        AlignWithTerrain = alignWithTerrain;

        // anything from here is unused in dawnlib but is here for vanilla compatibility
        SpawnFacingAwayFromWall = spawnFacingAwayFromWall;
        ObjectWidth = objectWidth;
        SpawnableFloorTags = spawnableFloorTags;
        RotationOffset = rotationOffset;
    }

    public ProviderTable<AnimationCurve?, DawnMoonInfo> SpawnWeights { get; private set; }
    public bool AlignWithTerrain { get; private set; }

    public bool SpawnFacingAwayFromWall { get; private set; }
    public int ObjectWidth { get; private set; }
    public string[] SpawnableFloorTags { get; private set; }
    public Vector3 RotationOffset { get; private set; }
}