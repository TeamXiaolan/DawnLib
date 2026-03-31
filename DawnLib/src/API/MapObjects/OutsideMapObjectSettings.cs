using System;
using UnityEngine;

namespace Dawn;

[Serializable]
public class OutsideMapObjectSettings
{
    public bool spawnFacingAwayFromWall = false;
    public int objectWidth = 6;
    public bool destroyTrees = false;
    public Vector3 rotationOffset = Vector3.zero;
    public string[] spawnableFloorTags = [];

    public int minimumAINodeSpawnRequirement = 0;
    public bool alignWithTerrain = false;
}