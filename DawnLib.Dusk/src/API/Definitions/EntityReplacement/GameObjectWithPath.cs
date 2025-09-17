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
    public Quaternion Rotation { get; private set; } = Quaternion.identity;
}