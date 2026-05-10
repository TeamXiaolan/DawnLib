
using System;
using UnityEngine;

namespace Dawn.Utils;

[Serializable]
public class AnimationEventData
{
    [field: SerializeField]
    public string AnimationEventName { get; internal set; }
    [field: SerializeField]
    public float Time { get; internal set; }

    [field: Header("Optional | Parameters")]
    [field: SerializeField]
    public string StringParameter { get; internal set; } = string.Empty;
    [field: SerializeField]
    public int IntParameter { get; internal set; } = 0;
    [field: SerializeField]
    public float FloatParameter { get; internal set; } = 0f;
    [field: SerializeField]
    public bool BoolParameter { get; internal set; } = false;
    [field: SerializeField]
    public UnityEngine.Object? ObjectParameter { get; internal set; } = null;
}