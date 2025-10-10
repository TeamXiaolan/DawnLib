using System;
using UnityEngine;

namespace Dusk;

[Serializable]
public class MaterialPropertiesWithIndex
{
    [field: SerializeField]
    public Texture2D? BaseMap { get; private set; }
    [field: SerializeField]
    public Texture2D? MaskMap { get; private set; }
    [field: SerializeField]
    public Texture2D? NormalMap { get; private set; }

    [field: SerializeField]
    public int Index { get; private set; }
}