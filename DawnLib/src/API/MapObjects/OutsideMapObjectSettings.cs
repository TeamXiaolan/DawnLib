using System;
using UnityEngine;

namespace Dawn;

[Serializable]
public class OutsideMapObjectSettings
{
    [HideInInspector]
    public bool SpawnFacingAwayFromWall;
    [HideInInspector]
    public int ObjectWidth;
    [HideInInspector]
    public string[] SpawnableFloorTags;
    [HideInInspector]
    public Vector3 RotationOffset;
    [HideInInspector]
    public AnimationCurve VanillaAnimationCurve;

    public bool AlignWithTerrain;
}