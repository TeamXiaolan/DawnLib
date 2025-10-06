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
    public AnimationEvent AnimationEventParameters { get; private set; }
}