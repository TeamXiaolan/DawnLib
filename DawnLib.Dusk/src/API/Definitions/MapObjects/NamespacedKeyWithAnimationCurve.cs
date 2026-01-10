using System;
using Dawn;
using UnityEngine;

namespace Dusk;
[Serializable]
public class NamespacedKeyWithAnimationCurve
{
    [field: SerializeField]
    [field: UnlockedNamespacedKey]
    [field: InspectorName("Namespace")]
    public NamespacedKey Key { get; private set; }

    [field: SerializeField]
    [field: Tooltip("Make sure the curve goes from 0 to 1 in the X-axis at most, nothing more nothing less, Y-axis is what determines how many can spawn.")]
    public AnimationCurve Curve { get; private set; }
}