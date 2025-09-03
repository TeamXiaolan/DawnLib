using System;
using UnityEngine;

namespace CodeRebirthLib.Utils;

[Serializable]
public class BoundedRange
{
    [field: SerializeField]
    public float Min { get; private set; }

    [field: SerializeField]
    public float Max { get; private set; }

    public BoundedRange(float min, float max)
    {
        Min = min;
        Max = max;

        if (Min > Max)
        {
            CodeRebirthLibPlugin.Logger.LogWarning($"Min ({Min}) is bigger than Max ({Max}), setting Min to {Max}");
            Min = Max;
        }
    }

    public bool IsInRange(float value) => value > Min && value <= Max;
    public float GetAverage() => (Min + Max) / 2f;
}