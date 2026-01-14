using System;
using UnityEngine;

namespace Dusk;

[Serializable]
public class GameObjectWithPath
{
    [field: SerializeField]
    public string PathToGameObject { get; private set; } = string.Empty;

    [field: SerializeField]
    public GameObject GameObjectToCreate { get; private set; }

    [field: SerializeField]
    public Vector3 PositionOffset { get; private set; } = Vector3.zero;

    [field: SerializeField]
    public Vector3 RotationOffset { get; private set; } = Vector3.zero;
}