using System;
using UnityEngine;

namespace Dawn;

[Serializable]
public class OutsideMapObjectSettings
{
    public bool SpawnFacingAwayFromWall = false;
    public int ObjectWidth = 6;
    public Vector3 RotationOffset = Vector3.zero;
    public string[] SpawnableFloorTags = [];

    public int MinimumAINodeSpawnRequirement = 0;
    public bool AlignWithTerrain = false;
}