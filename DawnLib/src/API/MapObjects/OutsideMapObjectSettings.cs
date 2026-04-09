using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dawn;

[Serializable]
public class OutsideMapObjectSettings
{
    [FormerlySerializedAs("SpawnFacingAwayFromWall")]
    public bool spawnFacingAwayFromWall = false;
    [FormerlySerializedAs("ObjectWidth")]
    public int objectWidth = 6;
    public bool destroyTrees = false;
    [FormerlySerializedAs("RotationOffset")]
    public Vector3 rotationOffset = Vector3.zero;
    [FormerlySerializedAs("SpawnableFloorTags")]
    public string[] spawnableFloorTags = [];

    [FormerlySerializedAs("MinimumAINodeSpawnRequirement")]
    public int minimumAINodeSpawnRequirement = 0;
    [FormerlySerializedAs("AlignWithTerrain")]
    public bool alignWithTerrain = false;
}