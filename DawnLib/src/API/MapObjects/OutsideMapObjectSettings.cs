using System;
using UnityEngine;

namespace Dawn;

[Serializable]
public class OutsideMapObjectSettings
{
    // TODO: implement these like in vanilla
    public bool SpawnFacingAwayFromWall = false;
    public int ObjectWidth = 6;
    public Vector3 RotationOffset = Vector3.zero;
    public string[] SpawnableFloorTags = [];

    // TODO: implement these too
    public int MinimumNodeSpawnRequirement = 0;
    public float MinimumDistanceFromShipAndEntrances = 0f;
    public bool AlignWithTerrain = false;
}