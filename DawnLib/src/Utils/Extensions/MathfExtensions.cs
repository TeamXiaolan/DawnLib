using UnityEngine;

namespace Dawn.Utils;

public static class MathfExtensions
{
    public static int Clamp0(this int value) => Mathf.Clamp(value, 0, int.MaxValue);
}