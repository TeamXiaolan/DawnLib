using System;
using Dawn;
using UnityEngine;

namespace Dusk;
[Serializable]
public class NamespacedKeyWithAnimationCurve
{
    [field: SerializeField]
    [field: UnlockedNamespacedKey]
    public NamespacedKey Key { get; private set; }

    [field: SerializeField]
    [field: Tooltip("Make sure you make the curve go from 0 to 1 in the X-axis at most, nothing more nothing less, Y-axis is what determines how many spawn.")]
    public AnimationCurve Curve { get; private set; }
}