using UnityEngine;

namespace Dawn;

public sealed class DawnOutsideMapObjectInfo
{
    public DawnMapObjectInfo ParentInfo { get; internal set; }

    internal DawnOutsideMapObjectInfo(ProviderTable<AnimationCurve?, DawnMoonInfo> spawnWeights, bool spawnFacingAwayFromWall, int objectWidth, string[] spawnableFloorTags, Vector3 rotationOffset, AnimationCurve vanillaAnimationCurve, bool alignWithTerrain)
    {
        SpawnWeights = spawnWeights;
        AlignWithTerrain = alignWithTerrain;

        // anything from here is unused in dawnlib but is here for vanilla compatibility
        SpawnFacingAwayFromWall = spawnFacingAwayFromWall;
        ObjectWidth = objectWidth;
        SpawnableFloorTags = spawnableFloorTags;
        RotationOffset = rotationOffset;
        VanillaAnimationCurve = vanillaAnimationCurve;
    }

    public ProviderTable<AnimationCurve?, DawnMoonInfo> SpawnWeights { get; }
    public bool AlignWithTerrain { get; }

    public bool SpawnFacingAwayFromWall { get; }
    public int ObjectWidth { get; }
    public string[] SpawnableFloorTags { get; }
    public Vector3 RotationOffset { get; }
    public AnimationCurve VanillaAnimationCurve { get; }
}