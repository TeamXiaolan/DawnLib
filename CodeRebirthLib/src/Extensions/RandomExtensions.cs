using System;
using CodeRebirthLib.Data;

namespace CodeRebirthLib.Extensions;
public static class RandomExtensions
{
    public static int Next(this Random random, BoundedRange range) {
        return (int)(random.NextDouble() * (range.Max - range.Min) + range.Min);
    }
    
    public static double NextDouble(this Random random, double min, double max)
    {
        return (random.NextDouble() * (max - min)) + min;
    }

    public static float NextFloat(this Random random, float min, float max)
    {
        return (float)random.NextDouble(min, max);
    }
}