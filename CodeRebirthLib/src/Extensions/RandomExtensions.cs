using System;
using CodeRebirthLib.Data;

namespace CodeRebirthLib.Extensions;
public static class RandomExtensions
{
    public static int Next(this Random random, BoundedRange range) {
        return (int)(random.NextDouble() * (range.Max - range.Min) + range.Min);
    }
}