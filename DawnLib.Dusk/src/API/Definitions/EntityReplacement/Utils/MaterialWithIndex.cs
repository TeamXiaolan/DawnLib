using System;
using Dawn;
using UnityEngine;

namespace Dusk;

[Serializable]
public class MaterialWithIndex
{
    [field: SerializeField]
    public Material Material { get; internal set; }
    [field: SerializeField]
    public int Index { get; internal set; }
}

[Serializable]
public class MaterialWithIndexListWithWeight : IWeighted
{
    [field: SerializeField]
    public MaterialWithIndex[] Materials { get; private set; }

    [field: SerializeField]
    public int Weight { get; private set; }

    public int GetWeight()
    {
        return Weight;
    }
}