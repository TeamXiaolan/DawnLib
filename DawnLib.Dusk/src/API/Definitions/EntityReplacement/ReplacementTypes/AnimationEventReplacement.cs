using System;
using UnityEngine;

namespace Dusk;

[Serializable]
public class AnimationEventAddition
{
    [field: SerializeField]
    public string AnimationEventName { get; private set; }
    [field: SerializeField]
    public float Time { get; private set; }

    [field: Header("Optional | Parameters")]
    [field: SerializeField]
    public string StringParameter { get; private set; } = string.Empty;
    [field: SerializeField]
    public int IntParameter { get; private set; } = 0;
    [field: SerializeField]
    public float FloatParameter { get; private set; } = 0f;
    [field: SerializeField]
    public bool BoolParameter { get; private set; } = false;
    [field: SerializeField]
    public UnityEngine.Object? ObjectParameter { get; private set; } = null;
}