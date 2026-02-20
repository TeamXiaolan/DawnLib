using UnityEngine;

namespace Dawn;

public sealed class DawnOutsideMapObjectInfo
{
    public DawnMapObjectInfo ParentInfo { get; internal set; }

    internal DawnOutsideMapObjectInfo(ProviderTable<AnimationCurve?, DawnMoonInfo, SpawnWeightContext> spawnWeights, bool spawnFacingAwayFromWall, int objectWidth, string[] spawnableFloorTags, Vector3 rotationOffset, bool alignWithTerrain, int minimumAINodeSpawnRequirement)
    {
        SpawnWeights = spawnWeights;
        AlignWithTerrain = alignWithTerrain;
        MinimumAINodeSpawnRequirement = minimumAINodeSpawnRequirement;

        SpawnFacingAwayFromWall = spawnFacingAwayFromWall;
        ObjectWidth = objectWidth;
        SpawnableFloorTags = spawnableFloorTags;
        RotationOffset = rotationOffset;
    }

    public ProviderTable<AnimationCurve?, DawnMoonInfo, SpawnWeightContext> SpawnWeights { get; private set; }
    public bool AlignWithTerrain { get; private set; }
    public int MinimumAINodeSpawnRequirement { get; private set; }

    public bool SpawnFacingAwayFromWall { get; private set; }
    public int ObjectWidth { get; private set; }
    public string[] SpawnableFloorTags { get; private set; }
    public Vector3 RotationOffset { get; private set; }
}