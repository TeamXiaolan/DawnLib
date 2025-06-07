using System;

namespace CodeRebirthLib.Data;

[Serializable]
public class BoundedRange
{
    public int Min { get; }
    public int Max { get; }

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