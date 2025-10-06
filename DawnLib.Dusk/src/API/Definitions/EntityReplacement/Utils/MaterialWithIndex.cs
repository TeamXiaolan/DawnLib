using System;
using UnityEngine;

namespace Dusk;

[Serializable]
public class MaterialWithIndex
{
    [field: SerializeField]
    public Material Material { get; private set; }
    [field: SerializeField]
    public int Index { get; private set; }
}