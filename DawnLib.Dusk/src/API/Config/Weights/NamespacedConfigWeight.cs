using System;
using Dawn;
using UnityEngine;

namespace Dusk.Weights;

[Serializable]
public class NamespacedConfigWeight : IOperationWithValue
{
    [field: SerializeField]
    [field: InspectorName("Namespace")]
    [field: UnlockedNamespacedKey]
    public NamespacedKey NamespacedKey;

    [field: SerializeField]
    public MathOperation MathOperation = MathOperation.Additive;

    [field: SerializeField]
    [field: Range(-9999, 9999)]
    public float Weight = 0;

    public MathOperation Operation => MathOperation;
    public float Value => Weight;
}