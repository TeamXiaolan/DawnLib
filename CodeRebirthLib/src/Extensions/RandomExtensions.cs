using System;
using System.Collections.Generic;
using System.Linq;
using CodeRebirthLib.Data;
using UnityEngine;
using Random = System.Random;

namespace CodeRebirthLib.Extensions;

public static class RandomExtensions
{
    public static float Next(this Random random, BoundedRange range)
    {
        return (float)(random.NextDouble() * (range.Max - range.Min) + range.Min);
    }

    public static double NextDouble(this Random random, double min, double max)
    {
        return random.NextDouble() * (max - min) + min;
    }

    public static float NextFloat(this Random random, float min, float max)
    {
        return (float)random.NextDouble(min, max);
    }

    public static T NextEnum<T>(this Random random) where T : struct, Enum
    {
        Array values = Enum.GetValues(typeof(T));
        return (T)values.GetValue(random.Next(values.Length));
    }

    public static T NextItem<T>(this Random random, IList<T> collection)
    {
        int index = random.Next(collection.Count);
        return collection[index];
    }

    public static bool NextBool(this Random random)
    {
        return random.Next(2) == 0;
    }

    public static int NextSign(this Random random)
    {
        return random.NextBool() ? 1 : -1;
    }

    public static Quaternion NextQuaternion(this Random random)
    {
        return Quaternion.Euler(random.NextFloat(0f, 360f), random.NextFloat(0f, 360f), random.NextFloat(0f, 360f));
    }

    public static T NextWeighted<T>(this Random random, IList<T> list) where T : IWeighted
    {
        int totalWeight = list.Sum(it => it.Weight);
        int chosenWeight = random.Next(0, totalWeight + 1);

        T chosen = list.First();
        foreach (T weighted in list)
        {
            chosen = weighted;
            chosenWeight -= weighted.Weight;
            if (chosenWeight <= 0)
                break;
        }
        return chosen;
    }
}