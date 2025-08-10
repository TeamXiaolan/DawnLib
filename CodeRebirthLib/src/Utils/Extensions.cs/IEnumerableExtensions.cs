using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace CodeRebirthLib;
public static class IEnumerableExtensions
{
    public static VertexGradient ToVertexGradient(this IEnumerable<Color> colors)
    {
        var list = colors.Take(4).ToList();
        int count = list.Count;

        Color c0, c1, c2, c3;
        switch (count)
        {
            case 0:
                c0 = c1 = c2 = c3 = Color.white;
                break;
            case 1:
                c0 = c1 = c2 = c3 = list[0];
                break;
            case 2:
                c0 = c1 = list[0];
                c2 = c3 = list[1];
                break;
            case 3:
                c0 = list[0];
                c1 = c2 = list[1];
                c3 = list[2];
                break;
            default:
                c0 = list[0];
                c1 = list[1];
                c2 = list[2];
                c3 = list[3];
                break;
        }

        return new VertexGradient(c0, c1, c2, c3);
    }
}