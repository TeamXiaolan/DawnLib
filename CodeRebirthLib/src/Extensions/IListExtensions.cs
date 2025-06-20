using System.Collections.Generic;
using UnityEngine;

namespace CodeRebirthLib.Extensions;
public static class IListExtensions
{ // Taken from Smooth_P in https://discussions.unity.com/t/clever-way-to-shuffle-a-list-t-in-one-line-of-c-code/535113
    /// <summary>
    ///     Shuffles the element order of the specified list.
    /// </summary>
    public static IList<T> Shuffle<T>(this IList<T> ts)
    {
        int count = ts.Count;
        int last = count - 1;
        for (int i = 0; i < last; ++i)
        {
            int r = Random.Range(i, count);
            (ts[i], ts[r]) = (ts[r], ts[i]);
        }

        return ts;
    }
}