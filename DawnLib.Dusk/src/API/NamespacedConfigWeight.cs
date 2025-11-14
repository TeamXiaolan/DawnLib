using System;
using Dawn;
using UnityEngine;

namespace Dusk;
[Serializable]
public class NamespacedConfigWeight
{
    [field: SerializeField]
    [field: InspectorName("Namespace")]
    public NamespacedKey NamespacedKey;

    [field: SerializeField]
    public MathOperation MathOperation = MathOperation.Additive;

    [field: SerializeField]
    public int Weight = 0;
}