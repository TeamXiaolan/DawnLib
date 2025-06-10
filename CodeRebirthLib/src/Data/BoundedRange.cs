using System;
using UnityEngine;

namespace CodeRebirthLib.Data;

[Serializable]
public class BoundedRange
{
    [field: SerializeField]
    public int Min { get; private set; }
    [field: SerializeField]
    public int Max { get; private set; }

    public BoundedRange(int min, int max)
    {
        Min = min;
        Max = max;
        
        if (Min > Max)
        {
            CodeRebirthLibPlugin.Logger.LogWarning($"Min ({Min}) is bigger than Max ({Max}), setting Min to {Max}");
            Min = Max;
        }
    }
}