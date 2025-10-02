using System;
using UnityEngine;

namespace Dusk;

[Serializable]
public class DawnDateTime
{
    [field: SerializeField]
    [field: Range(2025, 2050)]
    public int Year { get; private set; }

    [field: SerializeField]
    [field: Range(1, 12)]
    public int Month { get; private set; }

    [field: SerializeField]
    [field: Range(1, 31)]
    public int Day { get; private set; }

    [field: SerializeField]
    [field: Range(0, 23)]
    public int Hour { get; private set; }

    [field: SerializeField]
    [field: Range(0, 59)]
    public int Minute { get; private set; }

    [field: SerializeField]
    [field: Range(0, 59)]
    public int Second { get; private set; }
}