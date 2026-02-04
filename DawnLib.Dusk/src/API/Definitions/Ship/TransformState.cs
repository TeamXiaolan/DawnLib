using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Dusk;
public class TransformState
{
    public readonly Vector3 Position;
    public readonly Vector3 Scale;
    public readonly Quaternion Rotation;
    public readonly Transform Parent;

    public TransformState(Vector3 position, Vector3 scale, Quaternion rotation, Transform parent)
    {
        Position = position;
        Scale = scale;
        Rotation = rotation;
        Parent = parent;
    }
}
