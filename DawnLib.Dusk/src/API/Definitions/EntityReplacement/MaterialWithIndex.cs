using System;
using UnityEngine;

namespace Dusk;

[Serializable]
public class MaterialWithIndex
{
    [SerializeField]
    public Material Material { get; private set; }
    [SerializeField]
    public int Index { get; private set; }
}